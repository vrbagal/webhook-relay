using Microsoft.EntityFrameworkCore;
using WebhookRelay.Core.Entities;
using WebhookRelay.Core.Enums;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Infrastructure.Persistence;

public class WebhookEventRepository(AppDbContext db) : IWebhookEventRepository
{
    public Task<WebhookEvent?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Events
            .Include(e => e.Endpoint)
            .Include(e => e.DeliveryAttempts).ThenInclude(a => a.Target)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<(IReadOnlyList<WebhookEvent> Items, int TotalCount)> GetPagedAsync(
        Guid? endpointId, DeliveryStatus? status, string? eventType,
        string? providerEventId, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Events
            .Include(e => e.Endpoint)
            .Include(e => e.DeliveryAttempts)
            .AsQueryable();

        if (endpointId.HasValue)
            query = query.Where(e => e.EndpointId == endpointId);

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(e => e.EventType == eventType);

        if (!string.IsNullOrEmpty(providerEventId))
            query = query.Where(e => e.ProviderEventId == providerEventId);

        if (from.HasValue)
            query = query.Where(e => e.ReceivedAt >= from);

        if (to.HasValue)
            query = query.Where(e => e.ReceivedAt <= to);

        if (status.HasValue)
        {
            var statusStr = status.Value.ToString();
            query = query.Where(e =>
                e.DeliveryAttempts.OrderByDescending(a => a.AttemptNumber).First().Status.ToString() == statusStr);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.ReceivedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<bool> ExistsByProviderEventIdAsync(Guid endpointId, string providerEventId, CancellationToken ct) =>
        db.Events.AnyAsync(e => e.EndpointId == endpointId && e.ProviderEventId == providerEventId, ct);

    public async Task AddAsync(WebhookEvent webhookEvent, CancellationToken ct)
    {
        db.Events.Add(webhookEvent);
        await db.SaveChangesAsync(ct);
    }
}
