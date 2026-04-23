using WebhookRelay.Core.Entities;

namespace WebhookRelay.Core.Interfaces;

public interface IWebhookEndpointRepository
{
    Task<WebhookEndpoint?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<WebhookEndpoint>> GetAllAsync(CancellationToken ct = default);
    Task AddTargetAsync(DeliveryTarget target, CancellationToken ct);
    Task AddAsync(WebhookEndpoint endpoint, CancellationToken ct = default);
    Task UpdateAsync(WebhookEndpoint endpoint, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
