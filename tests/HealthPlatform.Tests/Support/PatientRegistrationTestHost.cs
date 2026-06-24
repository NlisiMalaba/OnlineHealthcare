using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthPlatform.Application;
using HealthPlatform.Application.Auth;
using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Application.Identity.UpdateDoctorProfile;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Search;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Auth;
using HealthPlatform.Infrastructure.Appointments;
using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.Outbox;
using HealthPlatform.Infrastructure.Persistence;
using HealthPlatform.Infrastructure.Persistence.Repositories;
using HealthPlatform.Infrastructure.Storage;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Tests.Support;

/// <summary>
/// In-memory Identity + patient registration stack for unit and integration tests.
/// </summary>
public sealed class PatientRegistrationTestHost : IAsyncDisposable
{
    public const string ValidPassword = "ValidPassw0rd!12";

    private readonly ServiceProvider _serviceProvider;

    private readonly TestCurrentUserAccessor _currentUser = new();

    private readonly string _databaseName = Guid.NewGuid().ToString("N");

    public TestCurrentUserAccessor CurrentUser => _currentUser;

    private readonly CapturingSearchService _searchService = new();

    public CapturingSearchService SearchService => _searchService;

    public PatientRegistrationTestHost()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        services.AddHttpContextAccessor();
        services.AddAuthentication();
        services.AddApplication();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(_databaseName));

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());
        services.Configure<SocialIdentityVerifierOptions>(options =>
        {
            options.AllowUnverifiedTokensInDevelopment = true;
        });
        services.Configure<StorageOptions>(options =>
        {
            options.LocalRootPath = Path.Combine(Path.GetTempPath(), "healthplatform-tests", Guid.NewGuid().ToString("N"));
        });

        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IHealthRecordRepository, HealthRecordRepository>();
        services.AddScoped<IHealthRecordProfileChangeRepository, HealthRecordProfileChangeRepository>();
        services.AddScoped<ISocialIdentityVerifier, SocialIdentityVerifier>();
        services.AddScoped<IPatientRegistrationWorkflow, PatientRegistrationWorkflow>();
        services.AddScoped<IPatientProfileUpdateWorkflow, PatientProfileUpdateWorkflow>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddSingleton<IAppointmentConfirmationNotifier, LoggingAppointmentConfirmationNotifier>();
        services.AddScoped<ILicenseVerificationQueueRepository, LicenseVerificationQueueRepository>();
        services.AddScoped<IDoctorRegistrationWorkflow, DoctorRegistrationWorkflow>();
        services.AddScoped<ILicenseVerificationWorkflow, LicenseVerificationWorkflow>();
        services.AddSingleton<IDoctorLicenseVerificationNotifier, LoggingDoctorLicenseVerificationNotifier>();
        services.AddScoped<IPharmacyRepository, PharmacyRepository>();
        services.AddScoped<IPharmacyRegistrationWorkflow, PharmacyRegistrationWorkflow>();
        services.AddScoped<IPharmacyProfileUpdateWorkflow, PharmacyProfileUpdateWorkflow>();
        services.AddScoped<IDoctorProfileUpdateWorkflow, DoctorProfileUpdateWorkflow>();
        services.AddSingleton<ISearchService>(_searchService);
        services.AddSingleton<ISlotHoldService, InMemorySlotHoldService>();
        services.AddSingleton<ICurrentUserAccessor>(_currentUser);
        services.AddSingleton<IStorageService, LocalFileStorageService>();

        _serviceProvider = services.BuildServiceProvider();
        SeedRolesAsync().GetAwaiter().GetResult();
    }

    public ISender Sender => _serviceProvider.GetRequiredService<ISender>();

    public ApplicationDbContext DbContext => _serviceProvider.GetRequiredService<ApplicationDbContext>();

    public T GetRequiredService<T>() where T : notnull =>
        _serviceProvider.GetRequiredService<T>();

    public static string CreateSocialIdToken(string subject, string? email = null, string? name = null)
    {
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, subject) };
        if (!string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, name));
        }

        var token = new JwtSecurityToken(claims: claims);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async ValueTask DisposeAsync() => await _serviceProvider.DisposeAsync();

    private async Task SeedRolesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "HealthPlatform.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
