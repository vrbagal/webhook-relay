using System.Diagnostics;
using System.Text;
using System.Text.Json;
using WebhookRelay.Core.Entities;
using WebhookRelay.Core.Enums;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Infrastructure.Delivery;

public class HttpDeliveryService(
    IHttpClientFactory httpClientFactory,
    IDeliveryAttemptRepository attemptRepo) : IDeliveryService
{
    private const int MaxAttempts = 5;

    public async Task DeliverAsync(WebhookEvent webhookEvent, DeliveryTarget target,
        int attemptNumber, bool isReplay, string? overrideUrl = null, CancellationToken ct = default)
    {
        // Skip delivery if routing rules don't match (replays bypass rule checks)
        if (!isReplay && !RoutingRuleEvaluator.Matches(webhookEvent.RawPayload, target.RoutingRules))
            return;

        var attempt = new DeliveryAttempt
        {
            EventId = webhookEvent.Id,
            TargetId = target.Id,
            AttemptNumber = attemptNumber,
            IsReplay = isReplay,
            Status = DeliveryStatus.Pending,
            AttemptedAt = DateTime.UtcNow,
        };

        await attemptRepo.AddAsync(attempt, ct);

        var sw = Stopwatch.StartNew();
        try
        {
            var client = httpClientFactory.CreateClient("delivery");
            var url = overrideUrl ?? target.TargetUrl;

            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(webhookEvent.HeadersJson)
                          ?? new Dictionary<string, string>();

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(webhookEvent.RawPayload, Encoding.UTF8, "application/json"),
            };

            request.Headers.TryAddWithoutValidation("X-Webhook-Relay-Event-Id", webhookEvent.Id.ToString());
            request.Headers.TryAddWithoutValidation("X-Webhook-Relay-Attempt", attemptNumber.ToString());

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(target.TimeoutSeconds));

            var response = await client.SendAsync(request, cts.Token);
            sw.Stop();

            attempt.HttpStatusCode = (int)response.StatusCode;
            attempt.ResponseBody = await response.Content.ReadAsStringAsync(CancellationToken.None);
            attempt.DurationMs = sw.ElapsedMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                attempt.Status = DeliveryStatus.Delivered;
            }
            else
            {
                attempt.Status = attemptNumber >= MaxAttempts
                    ? DeliveryStatus.DeadLettered
                    : DeliveryStatus.Failed;

                if (attempt.Status == DeliveryStatus.Failed)
                    attempt.NextRetryAt = DateTime.UtcNow.Add(GetBackoff(attemptNumber));
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            attempt.ErrorMessage = ex.Message;
            attempt.DurationMs = sw.ElapsedMilliseconds;
            attempt.Status = attemptNumber >= MaxAttempts
                ? DeliveryStatus.DeadLettered
                : DeliveryStatus.Failed;

            if (attempt.Status == DeliveryStatus.Failed)
                attempt.NextRetryAt = DateTime.UtcNow.Add(GetBackoff(attemptNumber));
        }

        await attemptRepo.UpdateAsync(attempt, ct);
    }

    private static TimeSpan GetBackoff(int attemptNumber) =>
        TimeSpan.FromSeconds(Math.Pow(2, attemptNumber) * 5);
}
