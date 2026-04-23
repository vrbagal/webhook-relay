using Microsoft.EntityFrameworkCore;
using WebhookRelay.Core.Entities;
using WebhookRelay.Core.Enums;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Infrastructure.Persistence;

public class DeliveryAttemptRepository(AppDbContext db) : IDeliveryAttemptRepository
{
    public async Task<IReadOnlyList<DeliveryAttempt>> GetByEventIdAsync(Guid eventId, CancellationToken ct)
    {
        return await db.DeliveryAttempts
            .Include(a => a.Target)
            .Where(a => a.EventId == eventId)
            .OrderBy(a => a.AttemptNumber)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DeliveryAttempt>> GetPendingForRetryAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return await db.DeliveryAttempts
            .Include(a => a.Event).ThenInclude(e => e.Endpoint)
            .Include(a => a.Target)
            .Where(a => a.Status == DeliveryStatus.Failed && a.NextRetryAt != null && a.NextRetryAt <= now)
            .ToListAsync(ct);
    }

    public async Task AddAsync(DeliveryAttempt attempt, CancellationToken ct)
    {
        db.DeliveryAttempts.Add(attempt);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DeliveryAttempt attempt, CancellationToken ct)
    {
        db.DeliveryAttempts.Update(attempt);
        await db.SaveChangesAsync(ct);
    }

    public Task<int> GetAttemptCountAsync(Guid eventId, Guid targetId, CancellationToken ct) =>
        db.DeliveryAttempts.CountAsync(a => a.EventId == eventId && a.TargetId == targetId, ct);
}
