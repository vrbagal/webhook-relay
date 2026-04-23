using WebhookRelay.Core.Entities;
using WebhookRelay.Core.Enums;

namespace WebhookRelay.Core.Interfaces;

public interface IDeliveryAttemptRepository
{
    Task<IReadOnlyList<DeliveryAttempt>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default);
    Task<IReadOnlyList<DeliveryAttempt>> GetPendingForRetryAsync(CancellationToken ct = default);
    Task AddAsync(DeliveryAttempt attempt, CancellationToken ct = default);
    Task UpdateAsync(DeliveryAttempt attempt, CancellationToken ct = default);
    Task<int> GetAttemptCountAsync(Guid eventId, Guid targetId, CancellationToken ct = default);
}
