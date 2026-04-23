using Microsoft.AspNetCore.Mvc;
using WebhookRelay.Core.Interfaces;
using WebhookRelay.Shared.DTOs;

namespace WebhookRelay.Api.Controllers;

[ApiController]
[Route("api/deliveries")]
public class DeliveriesController(IDeliveryAttemptRepository repo) : ControllerBase
{
    [HttpGet("event/{eventId:guid}")]
    public async Task<IActionResult> GetByEvent(Guid eventId, CancellationToken ct)
    {
        var attempts = await repo.GetByEventIdAsync(eventId, ct);
        var dtos = attempts.Select(a => new DeliveryAttemptDto(
            a.Id, a.EventId, a.TargetId, a.Target?.Name ?? string.Empty,
            a.Target?.TargetUrl ?? string.Empty, a.AttemptNumber, a.IsReplay,
            a.Status.ToString(), a.HttpStatusCode, a.ResponseBody,
            a.ErrorMessage, a.DurationMs, a.AttemptedAt, a.NextRetryAt));
        return Ok(dtos);
    }
}
