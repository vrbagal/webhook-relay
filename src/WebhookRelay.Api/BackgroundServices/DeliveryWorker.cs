using Microsoft.AspNetCore.SignalR;
using WebhookRelay.Api.Hubs;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Api.BackgroundServices;

public class DeliveryWorker(
    IWebhookChannel channel,
    IServiceScopeFactory scopeFactory,
    IHubContext<WebhookRelayHub> hubContext,
    ILogger<DeliveryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var webhookEvent in channel.ReadAllAsync(stoppingToken))
        {
            _ = Task.Run(() => ProcessEventAsync(webhookEvent, stoppingToken), stoppingToken);
        }
    }

    private async Task ProcessEventAsync(Core.Entities.WebhookEvent webhookEvent, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var endpointRepo = scope.ServiceProvider.GetRequiredService<IWebhookEndpointRepository>();
        var deliveryService = scope.ServiceProvider.GetRequiredService<IDeliveryService>();

        var endpoint = await endpointRepo.GetByIdAsync(webhookEvent.EndpointId, ct);
        if (endpoint is null) return;

        foreach (var target in endpoint.Targets.Where(t => t.IsActive))
        {
            try
            {
                await deliveryService.DeliverAsync(webhookEvent, target, 1, false, ct: ct);

                await hubContext.Clients.Group("all").SendAsync("DeliveryAttempted", new
                {
                    eventId = webhookEvent.Id,
                    status = "Delivered",
                    httpStatusCode = (int?)null,
                }, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to deliver event {EventId} to target {TargetId}",
                    webhookEvent.Id, target.Id);
            }
        }
    }
}
