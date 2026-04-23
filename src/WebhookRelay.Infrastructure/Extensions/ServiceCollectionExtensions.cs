using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebhookRelay.Core.Interfaces;
using WebhookRelay.Infrastructure.Delivery;
using WebhookRelay.Infrastructure.Persistence;
using WebhookRelay.Infrastructure.Verification;

namespace WebhookRelay.Infrastructure.Extensions;

/// <summary>
/// Supported database providers. Set via "DatabaseProvider" in appsettings.json.
/// Values: Sqlite (default) | SqlServer | PostgreSQL
/// </summary>
public enum DatabaseProvider
{
    Sqlite,
    SqlServer,
    PostgreSQL,
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var providerName = configuration.GetValue<string>("DatabaseProvider") ?? nameof(DatabaseProvider.Sqlite);

        if (!Enum.TryParse<DatabaseProvider>(providerName, ignoreCase: true, out var provider))
        {
            throw new InvalidOperationException(
                $"Unknown DatabaseProvider '{providerName}'. Valid values: Sqlite, SqlServer, PostgreSQL.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            var migrationsAssembly = typeof(AppDbContext).Assembly.FullName;

            switch (provider)
            {
                case DatabaseProvider.SqlServer:
                    options.UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly(migrationsAssembly));
                    break;

                case DatabaseProvider.PostgreSQL:
                    options.UseNpgsql(
                        configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly(migrationsAssembly));
                    break;

                case DatabaseProvider.Sqlite:
                default:
                    options.UseSqlite(
                        configuration.GetConnectionString("DefaultConnection") ?? "Data Source=webhookrelay.db",
                        b => b.MigrationsAssembly(migrationsAssembly));
                    break;
            }
        });

        services.AddScoped<IWebhookEndpointRepository, WebhookEndpointRepository>();
        services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
        services.AddScoped<IDeliveryAttemptRepository, DeliveryAttemptRepository>();
        services.AddScoped<IDeliveryService, HttpDeliveryService>();

        services.AddSingleton<IWebhookChannel, WebhookChannel>();

        services.AddSingleton<ISignatureVerifier, StripeVerifier>();
        services.AddSingleton<ISignatureVerifier, GitHubVerifier>();
        services.AddSingleton<ISignatureVerifier, TwilioVerifier>();
        services.AddSingleton<ISignatureVerifier, GenericVerifier>();

        services.AddHttpClient("delivery", client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WebhookRelay/1.0");
        });

        return services;
    }
}
