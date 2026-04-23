using Microsoft.EntityFrameworkCore;
using WebhookRelay.Core.Entities;
using WebhookRelay.Core.Enums;

namespace WebhookRelay.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WebhookEndpoint> Endpoints => Set<WebhookEndpoint>();
    public DbSet<WebhookEvent> Events => Set<WebhookEvent>();
    public DbSet<DeliveryTarget> DeliveryTargets => Set<DeliveryTarget>();
    public DbSet<DeliveryAttempt> DeliveryAttempts => Set<DeliveryAttempt>();
    public DbSet<RoutingRule> RoutingRules => Set<RoutingRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebhookEndpoint>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.SigningSecret).HasMaxLength(500).IsRequired();
            e.Property(x => x.Provider).HasConversion<string>();
            e.HasMany(x => x.Events).WithOne(x => x.Endpoint).HasForeignKey(x => x.EndpointId);
            e.HasMany(x => x.Targets).WithOne(x => x.Endpoint).HasForeignKey(x => x.EndpointId);
        });

        modelBuilder.Entity<WebhookEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RawPayload).IsRequired();
            e.Property(x => x.HeadersJson).IsRequired();
            e.Property(x => x.ProviderEventId).HasMaxLength(500);
            e.Property(x => x.EventType).HasMaxLength(200);
            e.HasIndex(x => new { x.EndpointId, x.ProviderEventId });
            e.HasMany(x => x.DeliveryAttempts).WithOne(x => x.Event).HasForeignKey(x => x.EventId);
        });

        modelBuilder.Entity<DeliveryTarget>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.TargetUrl).HasMaxLength(2000).IsRequired();
            e.HasMany(x => x.RoutingRules).WithOne(x => x.Target).HasForeignKey(x => x.TargetId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.DeliveryAttempts).WithOne(x => x.Target).HasForeignKey(x => x.TargetId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DeliveryAttempt>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => new { x.EventId, x.TargetId, x.AttemptNumber });
            e.HasIndex(x => x.NextRetryAt);
        });

        modelBuilder.Entity<RoutingRule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.JsonPath).HasMaxLength(500).IsRequired();
            e.Property(x => x.Operator).HasMaxLength(50).IsRequired();
            e.Property(x => x.Value).HasMaxLength(500);
        });
    }
}
