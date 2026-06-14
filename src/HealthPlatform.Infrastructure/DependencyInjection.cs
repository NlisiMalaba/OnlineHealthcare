using HealthPlatform.Application.Auth;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Storage;
using HealthPlatform.Infrastructure.Storage;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Security;
using HealthPlatform.Infrastructure.Auth;
using HealthPlatform.Infrastructure.Hosting;
using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.Jobs;
using HealthPlatform.Infrastructure.Outbox;
using HealthPlatform.Infrastructure.Persistence;
using HealthPlatform.Infrastructure.Persistence.Repositories;
using HealthPlatform.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Aes256AtRestEncryptionOptions>(
            configuration.GetSection(Aes256AtRestEncryptionOptions.SectionName));
        services.AddSingleton<IAtRestEncryption, Aes256AtRestEncryption>();
        services.AddScoped<IOutboxDomainEventDispatcher, OutboxDomainEventDispatcher>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IAccountLockoutService, AccountLockoutService>();
        services.AddSingleton<IAccountLockoutNotifier, LoggingAccountLockoutNotifier>();

        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redis);
        }

        var postgres = configuration.GetConnectionString("DefaultConnection");
        var useInMemoryDatabase = configuration.GetValue("Testing:UseInMemoryDatabase", false);
        if (!useInMemoryDatabase && string.IsNullOrWhiteSpace(postgres))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is required for Identity.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (useInMemoryDatabase)
            {
                options.UseInMemoryDatabase(
                    configuration["Testing:InMemoryDatabaseName"] ?? "HealthPlatform.Tests");
            }
            else
            {
                options.UseNpgsql(postgres!);
            }
        });

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<DeviceLoginOptions>(configuration.GetSection(DeviceLoginOptions.SectionName));
        services.Configure<SocialIdentityVerifierOptions>(
            configuration.GetSection(SocialIdentityVerifierOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserDeviceFingerprintRepository, UserDeviceFingerprintRepository>();
        services.AddScoped<IDeviceLoginVerificationRepository, DeviceLoginVerificationRepository>();
        services.AddSingleton<IDeviceLoginOtpNotifier, LoggingDeviceLoginOtpNotifier>();
        services.AddScoped<IAuthLoginWorkflow, AuthLoginWorkflow>();
        services.AddSingleton<IMfaSmsSender, LoggingMfaSmsSender>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IHealthRecordRepository, HealthRecordRepository>();
        services.AddScoped<IPatientRegistrationWorkflow, PatientRegistrationWorkflow>();
        services.AddScoped<IPatientProfileUpdateWorkflow, PatientProfileUpdateWorkflow>();
        services.AddScoped<ISocialIdentityVerifier, SocialIdentityVerifier>();
        services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();
        services.AddScoped<IHealthRecordProfileChangeRepository, HealthRecordProfileChangeRepository>();
        services.AddSingleton<IStorageService, LocalFileStorageService>();
        services.AddHttpContextAccessor();

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(ConfigureIdentity)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddHostedService<IdentityDataSeedingHostedService>();

        services.AddTransient<OutboxProcessorJob>();
        services.AddTransient<ScheduledRemindersJob>();
        return services;
    }

    private static void ConfigureIdentity(IdentityOptions options)
    {
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.SignIn.RequireConfirmedEmail = false;
        options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
        options.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultPhoneProvider;
    }
}
