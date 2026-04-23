using Microsoft.EntityFrameworkCore;
using WebhookRelay.Core.Entities;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Infrastructure.Persistence;

public class WebhookEndpointRepository(AppDbContext db) : IWebhookEndpointRepository
{
    public Task<WebhookEndpoint?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Endpoints.Include(e => e.Targets).ThenInclude(t => t.RoutingRules)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<WebhookEndpoint>> GetAllAsync(CancellationToken ct)
    {
        var list = await db.Endpoints
            .Include(e => e.Targets).ThenInclude(t => t.RoutingRules)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
        return list;
    }

    public async Task AddAsync(WebhookEndpoint endpoint, CancellationToken ct)
    {
        db.Endpoints.Add(endpoint);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddTargetAsync(DeliveryTarget target, CancellationToken ct)
    {
        db.DeliveryTargets.Add(target);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(WebhookEndpoint endpoint, CancellationToken ct)
    {
        // The endpoint was loaded from this same scoped DbContext so EF Core
        // is already tracking all changes — including newly added child entities
        // (targets, routing rules). Calling Update() here would incorrectly mark
        // new children as Modified instead of Added, causing a concurrency error.
        // Just save the tracked changes directly.
        if (db.Entry(endpoint).State == EntityState.Detached)
            db.Endpoints.Update(endpoint);   // only attach if truly detached

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.Endpoints.FindAsync([id], ct);
        if (entity is not null)
        {
            db.Endpoints.Remove(entity);
            await db.SaveChangesAsync(ct);
        }
    }
}
