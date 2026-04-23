using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using WebhookRelay.Api.BackgroundServices;
using WebhookRelay.Api.Hubs;
using WebhookRelay.Api.Middleware;
using WebhookRelay.Infrastructure.Extensions;   // DatabaseProvider enum lives here
using WebhookRelay.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console()
      .WriteTo.File("logs/webhookrelay-.log", rollingInterval: RollingInterval.Day));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<DeliveryWorker>();
builder.Services.AddHostedService<RetryWorker>();

builder.Services.AddCors(options =>
    options.AddPolicy("Dev", policy =>
        policy.WithOrigins("http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebhookRelay"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

var app = builder.Build();

// ── Database initialisation ──────────────────────────────────────────────────
// SQLite: EnsureCreated is sufficient — no migration history table needed for
//         a file-based dev/personal database.
// SQL Server / PostgreSQL: apply pending EF Core migrations on startup so
//         production deployments stay schema-current automatically.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var providerName = app.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
    _ = Enum.TryParse<DatabaseProvider>(providerName, ignoreCase: true, out var dbProvider);

    logger.LogInformation("Database provider: {Provider}", dbProvider);

    if (dbProvider == DatabaseProvider.Sqlite)
    {
        // SQLite: EnsureCreated is sufficient — no migration history table needed
        // for a file-based dev/personal database.
        await db.Database.EnsureCreatedAsync();
        logger.LogInformation("SQLite database ready at: {DataSource}", db.Database.GetConnectionString());
    }
    else
    {
        // SQL Server / PostgreSQL: apply pending EF Core migrations on startup.
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Dev");
}

app.UseMiddleware<RawBodyMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHub<WebhookRelayHub>("/hubs/webhookrelay");

app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
