namespace WebhookRelay.Core.Entities;

public class DeliveryTarget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EndpointId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;

    public WebhookEndpoint Endpoint { get; set; } = null!;
    public ICollection<RoutingRule> RoutingRules { get; set; } = new List<RoutingRule>();
    public ICollection<DeliveryAttempt> DeliveryAttempts { get; set; } = new List<DeliveryAttempt>();
}
