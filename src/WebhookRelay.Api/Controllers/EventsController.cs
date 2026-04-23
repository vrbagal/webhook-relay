using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebhookRelay.Core.Enums;
using WebhookRelay.Core.Interfaces;
using WebhookRelay.Shared.DTOs;
using WebhookRelay.Shared.Models;

namespace WebhookRelay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(
    IWebhookEventRepository eventRepo,
    IDeliveryAttemptRepository attemptRepo,
    IWebhookEndpointRepository endpointRepo,
    IDeliveryService deliveryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? endpointId,
        [FromQuery] string? status,
        [FromQuery] string? eventType,
        [FromQuery] string? providerEventId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        DeliveryStatus? deliveryStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<DeliveryStatus>(status, true, out var parsed))
            deliveryStatus = parsed;

        var (items, totalCount) = await eventRepo.GetPagedAsync(
            endpointId, deliveryStatus, eventType, providerEventId, from, to, page, pageSize, ct);

        var dtos = items.Select(MapToDto).ToList();
        return Ok(new PagedResult<WebhookEventDto>(dtos, totalCount, page, pageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var evt = await eventRepo.GetByIdAsync(id, ct);
        return evt is null ? NotFound() : Ok(MapToDto(evt));
    }

    [HttpPost("{id:guid}/replay")]
    public async Task<IActionResult> Replay(Guid id, [FromBody] ReplayRequest? request, CancellationToken ct)
    {
        var evt = await eventRepo.GetByIdAsync(id, ct);
        if (evt is null) return NotFound();

        var endpoint = await endpointRepo.GetByIdAsync(evt.EndpointId, ct);
        if (endpoint is null) return NotFound();

        foreach (var target in endpoint.Targets.Where(t => t.IsActive))
        {
            var attemptCount = await attemptRepo.GetAttemptCountAsync(id, target.Id, ct);
            await deliveryService.DeliverAsync(
                evt, target, attemptCount + 1, isReplay: true,
                overrideUrl: request?.OverrideTargetUrl, ct: ct);
        }

        return Accepted();
    }

    private static WebhookEventDto MapToDto(Core.Entities.WebhookEvent e)
    {
        Dictionary<string, string> headers;
        try { headers = JsonSerializer.Deserialize<Dictionary<string, string>>(e.HeadersJson) ?? []; }
        catch { headers = []; }

        return new WebhookEventDto(
            e.Id, e.EndpointId, e.Endpoint?.Name ?? string.Empty,
            e.RawPayload, headers, e.ProviderEventId, e.EventType,
            e.SignatureVerified, e.IsDuplicate, e.ReceivedAt,
            e.DeliveryAttempts.Select(a => new DeliveryAttemptDto(
                a.Id, a.EventId, a.TargetId, a.Target?.Name ?? string.Empty,
                a.Target?.TargetUrl ?? string.Empty, a.AttemptNumber, a.IsReplay,
                a.Status.ToString(), a.HttpStatusCode, a.ResponseBody,
                a.ErrorMessage, a.DurationMs, a.AttemptedAt, a.NextRetryAt
            )).ToList());
    }
}
