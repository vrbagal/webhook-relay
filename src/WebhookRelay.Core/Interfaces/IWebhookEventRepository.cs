using WebhookRelay.Core.Entities;
using WebhookRelay.Core.Enums;

namespace WebhookRelay.Core.Interfaces;

public interface IWebhookEventRepository
{
    Task<WebhookEvent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<WebhookEvent> Items, int TotalCount)> GetPagedAsync(
        Guid? endpointId, DeliveryStatus? status, string? eventType,
        string? providerEventId, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsByProviderEventIdAsync(Guid endpointId, string providerEventId, CancellationToken ct = default);
    Task AddAsync(WebhookEvent webhookEvent, CancellationToken ct = default);
}
