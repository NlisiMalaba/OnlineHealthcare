using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.PostgreSql;
using HealthPlatform.API.Authorization;
using HealthPlatform.API.Diagnostics;
using HealthPlatform.API.Middleware;
using HealthPlatform.Application;
using HealthPlatform.Infrastructure;
using HealthPlatform.Infrastructure.Jobs;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthPlatformAuthorizationPolicies();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.Configure<RequireTlsMiddlewareOptions>(
    builder.Configuration.GetSection("Security:Tls"));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var hangfireConnection = builder.Configuration.GetConnectionString("Hangfire")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'Hangfire' or 'DefaultConnection' is not configured.");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(hangfireConnection)));

builder.Services.AddHangfireServer();

var postgres = builder.Configuration.GetConnectionString("DefaultConnection");
var redis = builder.Configuration.GetConnectionString("Redis");
var mongo = builder.Configuration.GetConnectionString("MongoDb");
var elasticsearchUri = builder.Configuration["Elasticsearch:Uri"];

var health = builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

if (!string.IsNullOrWhiteSpace(postgres))
{
    health.AddNpgSql(postgres, name: "postgres", tags: ["ready"]);
}

if (!string.IsNullOrWhiteSpace(redis))
{
    health.AddRedis(redis, name: "redis", tags: ["ready"]);
}

if (!string.IsNullOrWhiteSpace(mongo))
{
    health.AddMongoDb(
        _ => new MongoClient(mongo).GetDatabase("admin"),
        name: "mongodb",
        tags: ["ready"]);
}

if (!string.IsNullOrWhiteSpace(elasticsearchUri))
{
    health.AddElasticsearch(elasticsearchUri, name: "elasticsearch", tags: ["ready"]);
}

if (builder.Configuration.GetValue("OpenTelemetry:Enabled", true))
{
    var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "HealthPlatform.API";
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService(serviceName))
        .WithTracing(t => t
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter());
}

builder.Services.AddControllers();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var key = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseMiddleware<RequireTlsMiddleware>();
if (!string.IsNullOrWhiteSpace(redis))
{
    app.UseMiddleware<IdempotencyMiddleware>();
}

app.UseSerilogRequestLogging();

app.UseRateLimiter();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        RecurringJob.AddOrUpdate<OutboxProcessorJob>(
            "outbox-processor",
            job => job.Run(),
            Cron.MinuteInterval(5));

        RecurringJob.AddOrUpdate<ScheduledRemindersJob>(
            "scheduled-reminders",
            job => job.Run(),
            Cron.MinuteInterval(15));
    }
    catch (Exception ex)
    {
        Log.Warning(
            ex,
            "Hangfire recurring jobs were not registered; start PostgreSQL (see docker-compose.yml) and restart.");
    }
});

app.Run();
