using Microsoft.AspNetCore.SignalR;
using WebhookRelay.Api.Hubs;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Api.BackgroundServices;

public class RetryWorker(
    IServiceScopeFactory scopeFactory,
    IHubContext<WebhookRelayHub> hubContext,
    ILogger<RetryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessRetries(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task ProcessRetries(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var attemptRepo = scope.ServiceProvider.GetRequiredService<IDeliveryAttemptRepository>();
        var eventRepo = scope.ServiceProvider.GetRequiredService<IWebhookEventRepository>();
        var deliveryService = scope.ServiceProvider.GetRequiredService<IDeliveryService>();

        var pendingRetries = await attemptRepo.GetPendingForRetryAsync(ct);

        foreach (var attempt in pendingRetries)
        {
            try
            {
                var webhookEvent = await eventRepo.GetByIdAsync(attempt.EventId, ct);
                if (webhookEvent is null) continue;

                var nextAttemptNumber = attempt.AttemptNumber + 1;
                await deliveryService.DeliverAsync(
                    webhookEvent, attempt.Target, nextAttemptNumber, false, ct: ct);

                await hubContext.Clients.Group("all").SendAsync("DeliveryAttempted", new
                {
                    eventId = attempt.EventId,
                    status = "Retried",
                    httpStatusCode = (int?)null,
                }, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed retry for attempt {AttemptId}", attempt.Id);
            }
        }
    }
}
