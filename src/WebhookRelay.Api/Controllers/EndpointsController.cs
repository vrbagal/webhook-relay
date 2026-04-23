using Microsoft.AspNetCore.Mvc;
using WebhookRelay.Core.Entities;
using WebhookRelay.Core.Enums;
using WebhookRelay.Core.Interfaces;
using WebhookRelay.Shared.DTOs;

namespace WebhookRelay.Api.Controllers;

[ApiController]
[Route("api/endpoints")]
public class EndpointsController(
    IWebhookEndpointRepository repo,
    IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    private static readonly string[] ValidOperators =
    [
        "equals", "not_equals", "contains", "not_contains",
        "exists", "not_exists", "starts_with", "ends_with",
    ];

    // ── Endpoints ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var endpoints = await repo.GetAllAsync(ct);
        return Ok(endpoints.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var endpoint = await repo.GetByIdAsync(id, ct);
        return endpoint is null ? NotFound() : Ok(MapToDto(endpoint));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEndpointRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<ProviderType>(request.Provider, true, out var provider))
            return BadRequest(new { detail = $"Unknown provider: {request.Provider}" });

        var endpoint = new WebhookEndpoint
        {
            Name = request.Name,
            Provider = provider,
            SigningSecret = request.SigningSecret,
            RejectUnverified = request.RejectUnverified,
        };

        await repo.AddAsync(endpoint, ct);
        return CreatedAtAction(nameof(GetById), new { id = endpoint.Id }, MapToDto(endpoint));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEndpointRequest request, CancellationToken ct)
    {
        var endpoint = await repo.GetByIdAsync(id, ct);
        if (endpoint is null) return NotFound();

        endpoint.Name = request.Name;
        endpoint.IsActive = request.IsActive;
        endpoint.RejectUnverified = request.RejectUnverified;
        if (!string.IsNullOrEmpty(request.SigningSecret))
            endpoint.SigningSecret = request.SigningSecret;

        await repo.UpdateAsync(endpoint, ct);
        return Ok(MapToDto(endpoint));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await repo.DeleteAsync(id, ct);
        return NoContent();
    }

    // ── Targets ──────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/targets")]
    public async Task<IActionResult> AddTarget(Guid id, [FromBody] CreateTargetRequest request, CancellationToken ct)
    {
        var endpoint = await repo.GetByIdAsync(id, ct);
        if (endpoint is null) return NotFound();

        var target = new DeliveryTarget
        {
            EndpointId = id,
            Name = request.Name,
            TargetUrl = request.TargetUrl,
            TimeoutSeconds = request.TimeoutSeconds,
        };

        endpoint.Targets.Add(target);
        await repo.AddTargetAsync(target, ct);
        return Ok(MapTargetToDto(target));
    }

    [HttpDelete("{id:guid}/targets/{targetId:guid}")]
    public async Task<IActionResult> DeleteTarget(Guid id, Guid targetId, CancellationToken ct)
    {
        var endpoint = await repo.GetByIdAsync(id, ct);
        if (endpoint is null) return NotFound();

        var target = endpoint.Targets.FirstOrDefault(t => t.Id == targetId);
        if (target is null) return NotFound();

        endpoint.Targets.Remove(target);
        await repo.UpdateAsync(endpoint, ct);
        return NoContent();
    }

    // ── Routing rules ────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/targets/{targetId:guid}/rules")]
    public async Task<IActionResult> GetRules(Guid id, Guid targetId, CancellationToken ct)
    {
        var endpoint = await repo.GetByIdAsync(id, ct);
        if (endpoint is null) return NotFound();

        var target = endpoint.Targets.FirstOrDefault(t => t.Id == targetId);
        if (target is null) return NotFound();

        return Ok(target.RoutingRules.Select(MapRuleToDto));
    }

    [HttpPost("{id:guid}/targets/{targetId:guid}/rules")]
    public async Task<IActionResult> AddRule(
        Guid id, Guid targetId,
        [FromBody] CreateRoutingRuleRequest request,
        CancellationToken ct)
    {
        if (!ValidOperators.Contains(request.Operator.ToLowerInvariant()))
            return BadRequest(new { detail = $"Unknown operator '{request.Operator}'. Valid: {string.Join(", ", ValidOperators)}" });

        var endpoint = await repo.GetByIdAsync(id, ct);
        if (endpoint is null) return NotFound();

        var target = endpoint.Targets.FirstOrDefault(t => t.Id == targetId);
        if (target is null) return NotFound();

        var rule = new RoutingRule
        {
            TargetId = targetId,
            JsonPath = request.JsonPath.Trim(),
            Operator = request.Operator.ToLowerInvariant(),
            Value = request.Value,
        };

        target.RoutingRules.Add(rule);
        await repo.UpdateAsync(endpoint, ct);
        return Ok(MapRuleToDto(rule));
    }

    [HttpDelete("{id:guid}/targets/{targetId:guid}/rules/{ruleId:guid}")]
    public async Task<IActionResult> DeleteRule(Guid id, Guid targetId, Guid ruleId, CancellationToken ct)
    {
        var endpoint = await repo.GetByIdAsync(id, ct);
        if (endpoint is null) return NotFound();

        var target = endpoint.Targets.FirstOrDefault(t => t.Id == targetId);
        if (target is null) return NotFound();

        var rule = target.RoutingRules.FirstOrDefault(r => r.Id == ruleId);
        if (rule is null) return NotFound();

        target.RoutingRules.Remove(rule);
        await repo.UpdateAsync(endpoint, ct);
        return NoContent();
    }

    // ── Mappers ──────────────────────────────────────────────────────────────

    private WebhookEndpointDto MapToDto(WebhookEndpoint e) => new(
        e.Id, e.Name, e.Provider.ToString(), e.IsActive, e.RejectUnverified,
        BuildIngestUrl(e.Id), e.CreatedAt,
        e.Targets.Select(MapTargetToDto).ToList());

    private static DeliveryTargetDto MapTargetToDto(DeliveryTarget t) => new(
        t.Id, t.EndpointId, t.Name, t.TargetUrl, t.IsActive, t.TimeoutSeconds,
        t.RoutingRules.Select(MapRuleToDto).ToList());

    private static RoutingRuleDto MapRuleToDto(RoutingRule r) =>
        new(r.Id, r.TargetId, r.JsonPath, r.Operator, r.Value);

    private string BuildIngestUrl(Guid endpointId)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null) return $"/webhooks/{endpointId}";
        return $"{request.Scheme}://{request.Host}/webhooks/{endpointId}";
    }
}
