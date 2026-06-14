using HealthPlatform.Application.Auth;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Security;
using HealthPlatform.Infrastructure.Auth;
using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.Outbox;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Tests.Support;

/// <summary>
/// In-memory Identity + auth workflow for unit and property tests.
/// </summary>
public sealed class AuthTestHost : IAsyncDisposable
{
    public const string ValidPassword = "ValidPassw0rd!12";

    public const string WrongPassword = "WrongPassw0rd!99";

    public const string DeviceFingerprint = "test-device-fingerprint-abcdef01";

    private readonly ServiceProvider _serviceProvider;

    private readonly string _databaseName = Guid.NewGuid().ToString("N");

    public CapturingDeviceLoginOtpNotifier DeviceOtpNotifier { get; }

    public AuthTestHost()
    {
        DeviceOtpNotifier = new CapturingDeviceLoginOtpNotifier();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        services.AddHttpContextAccessor();
        services.AddAuthentication();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(_databaseName));

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(ConfigureIdentity)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = "HealthPlatform.Tests";
            options.Audience = "HealthPlatform.Tests";
            options.SigningKey = "TEST_SIGNING_KEY________________________________________________________________";
            options.AccessTokenMinutes = 60;
            options.MfaChallengeMinutes = 5;
        });

        services.Configure<DeviceLoginOptions>(options => options.ChallengeTtlMinutes = 10);

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IAccountLockoutService, AccountLockoutService>();
        services.AddScoped<IUserDeviceFingerprintRepository, UserDeviceFingerprintRepository>();
        services.AddScoped<IDeviceLoginVerificationRepository, DeviceLoginVerificationRepository>();
        services.AddSingleton<IDeviceLoginOtpNotifier>(DeviceOtpNotifier);
        services.AddScoped<IAuthLoginWorkflow, AuthLoginWorkflow>();

        _serviceProvider = services.BuildServiceProvider();
        SeedRolesAsync().GetAwaiter().GetResult();
    }

    public async Task<ApplicationUser> CreateUserAsync(
        string email,
        string password,
        IReadOnlyList<string> roles,
        bool twoFactorEnabled = false,
        CancellationToken ct = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var create = await userManager.CreateAsync(user, password);
        if (!create.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        foreach (var role in roles)
        {
            var addRole = await userManager.AddToRoleAsync(user, role);
            if (!addRole.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", addRole.Errors.Select(e => e.Description)));
            }
        }

        user = await userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException("User was not persisted.");

        if (twoFactorEnabled)
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            await userManager.SetTwoFactorEnabledAsync(user, true);
        }

        return user;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginCommand command, CancellationToken ct = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        EnsureSignInHttpContext(scope);
        var workflow = scope.ServiceProvider.GetRequiredService<IAuthLoginWorkflow>();
        return await workflow.LoginAsync(command, ct);
    }

    public async Task<LoginResponseDto> CompleteMfaAsync(CompleteMfaLoginCommand command, CancellationToken ct = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        EnsureSignInHttpContext(scope);
        var workflow = scope.ServiceProvider.GetRequiredService<IAuthLoginWorkflow>();
        return await workflow.CompleteMfaAsync(command, ct);
    }

    public async Task<bool> IsUserLockedOutAsync(string email, CancellationToken ct = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        return user is not null && await userManager.IsLockedOutAsync(user);
    }

    public async Task<int> CountAccountLockedOutboxEventsAsync(CancellationToken ct = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.DomainEventOutbox.CountAsync(
            x => x.EventType == "HealthPlatform.Domain.Identity.Events.AccountLockedDomainEvent",
            ct);
    }

    public async Task TrustDeviceAsync(Guid userId, string deviceFingerprint, CancellationToken ct = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserDeviceFingerprintRepository>();
        var hash = DeviceFingerprintHasher.Hash(deviceFingerprint);
        await repository.UpsertTouchAsync(userId, hash, ct);
    }

    public async Task<string> CreateMfaChallengeTokenAsync(Guid userId, CancellationToken ct = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        return jwt.CreateMfaChallengeToken(userId, ct);
    }

    public async Task<bool> IsTwoFactorEnabledAsync(string email, CancellationToken ct = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        return user is not null && await userManager.GetTwoFactorEnabledAsync(user);
    }

    public async Task<string> GenerateAuthenticatorCodeAsync(string email, CancellationToken ct = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException("User not found.");
        return await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider);
    }

    public ValueTask DisposeAsync()
    {
        _serviceProvider.Dispose();
        return ValueTask.CompletedTask;
    }

    private static void EnsureSignInHttpContext(IServiceScope scope)
    {
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        signInManager.Context = accessor.HttpContext;
    }

    private async Task SeedRolesAsync()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var roleName in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
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
    }
}
