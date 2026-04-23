using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebhookRelay.Api.Hubs;
using WebhookRelay.Core.Entities;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Api.Controllers;

[ApiController]
[Route("webhooks")]
public class WebhooksController(
    IWebhookEndpointRepository endpointRepo,
    IWebhookEventRepository eventRepo,
    IWebhookChannel channel,
    IEnumerable<ISignatureVerifier> verifiers,
    IHubContext<WebhookRelayHub> hubContext,
    ILogger<WebhooksController> logger) : ControllerBase
{
    [HttpPost("{endpointId:guid}")]
    public async Task<IActionResult> Ingest(Guid endpointId, CancellationToken ct)
    {
        var endpoint = await endpointRepo.GetByIdAsync(endpointId, ct);
        if (endpoint is null || !endpoint.IsActive)
            return NotFound();

        var rawBody = HttpContext.Items["RawBody"] as string ?? string.Empty;
        var headers = Request.Headers
            .ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value.ToString());

        var verifier = verifiers.FirstOrDefault(v => v.CanHandle(endpoint.Provider.ToString()));
        var signatureVerified = verifier?.Verify(rawBody, headers, endpoint.SigningSecret) ?? false;

        var providerEventId = verifier?.ExtractEventId(rawBody, headers);
        var eventType = verifier?.ExtractEventType(rawBody, headers);

        var isDuplicate = providerEventId is not null
            && await eventRepo.ExistsByProviderEventIdAsync(endpointId, providerEventId, ct);

        var webhookEvent = new WebhookEvent
        {
            EndpointId = endpointId,
            RawPayload = rawBody,
            HeadersJson = JsonSerializer.Serialize(headers),
            ProviderEventId = providerEventId,
            EventType = eventType,
            SignatureVerified = signatureVerified,
            IsDuplicate = isDuplicate,
        };

        await eventRepo.AddAsync(webhookEvent, ct);

        await hubContext.Clients.Group("all").SendAsync("EventReceived", new { id = webhookEvent.Id }, ct);

        // Reject after storing so the event is always auditable in the dashboard
        if (endpoint.RejectUnverified && !signatureVerified)
        {
            logger.LogWarning("Rejected unverified webhook for endpoint {EndpointId} (event {EventId} stored for audit)",
                endpointId, webhookEvent.Id);
            return Unauthorized(new { id = webhookEvent.Id, detail = "Signature verification failed" });
        }

        if (!isDuplicate)
            channel.Enqueue(webhookEvent);

        return Accepted(new { id = webhookEvent.Id });
    }
}
