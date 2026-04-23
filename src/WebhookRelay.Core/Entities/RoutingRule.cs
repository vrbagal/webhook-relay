namespace WebhookRelay.Core.Entities;

public class RoutingRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TargetId { get; set; }
    public string JsonPath { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string? Value { get; set; }

    public DeliveryTarget Target { get; set; } = null!;
}
