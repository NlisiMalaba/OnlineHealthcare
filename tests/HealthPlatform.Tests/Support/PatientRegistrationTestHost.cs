using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthPlatform.Application;
using HealthPlatform.Application.Auth;
using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Application.PharmacyOrders.Realtime;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.DrugInteractions;
using HealthPlatform.Application.Wellness;
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
using HealthPlatform.Infrastructure.Prescriptions;
using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.Outbox;
using HealthPlatform.Infrastructure.Persistence;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.Realtime;
using HealthPlatform.Infrastructure.Telemedicine;
using HealthPlatform.Infrastructure.MongoDb;
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

    private readonly CapturingTelemedicineRealtimeNotifier _telemedicineRealtimeNotifier = new();

    public CapturingTelemedicineRealtimeNotifier TelemedicineRealtimeNotifier => _telemedicineRealtimeNotifier;

    private readonly CapturingPharmacyOrderRealtimeNotifier _pharmacyOrderRealtimeNotifier = new();

    public CapturingPharmacyOrderRealtimeNotifier PharmacyOrderRealtimeNotifier => _pharmacyOrderRealtimeNotifier;

    private readonly CapturingPharmacyOrderReceivedNotifier _pharmacyOrderReceivedNotifier = new();

    public CapturingPharmacyOrderReceivedNotifier PharmacyOrderReceivedNotifier => _pharmacyOrderReceivedNotifier;

    private readonly FakePharmacyStockAvailabilityService _pharmacyStockAvailability = new();

    public FakePharmacyStockAvailabilityService PharmacyStockAvailability => _pharmacyStockAvailability;

    public PatientRegistrationTestHost(
        IAppointmentConfirmationNotifier? appointmentConfirmationNotifier = null,
        IAppointmentRescheduleNotifier? appointmentRescheduleNotifier = null,
        IPrescriptionIssuedNotifier? prescriptionIssuedNotifier = null,
        IPrescriptionCancelledNotifier? prescriptionCancelledNotifier = null,
        IDrugInteractionAlertNotifier? drugInteractionAlertNotifier = null,
        FakeTimeProvider? timeProvider = null)
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
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        services.AddScoped<IMedicationScheduleRepository, MedicationScheduleRepository>();
        services.AddSingleton<IDrugInteractionChecker, StaticDrugInteractionChecker>();
        if (prescriptionIssuedNotifier is not null)
        {
            services.AddSingleton(prescriptionIssuedNotifier);
        }
        else
        {
            services.AddSingleton<IPrescriptionIssuedNotifier, LoggingPrescriptionIssuedNotifier>();
        }

        if (prescriptionCancelledNotifier is not null)
        {
            services.AddSingleton(prescriptionCancelledNotifier);
        }
        else
        {
            services.AddSingleton<IPrescriptionCancelledNotifier, LoggingPrescriptionCancelledNotifier>();
        }

        if (drugInteractionAlertNotifier is not null)
        {
            services.AddSingleton(drugInteractionAlertNotifier);
        }
        else
        {
            services.AddSingleton<IDrugInteractionAlertNotifier, LoggingDrugInteractionAlertNotifier>();
        }

        services.Configure<RtcOptions>(options => { });
        services.AddSingleton<IRtcProviderResolver, ConfigurableRtcProviderResolver>();
        services.AddSingleton<IRtcTokenService, RtcTokenService>();
        services.AddScoped<ITelemedicineSessionRepository, TelemedicineSessionRepository>();
        services.AddScoped<ITelemedicineSessionParticipantService, TelemedicineSessionParticipantService>();
        services.AddSingleton<ITelemedicineRealtimeNotifier>(_telemedicineRealtimeNotifier);
        services.AddSingleton<InMemoryTelemedicineSessionSummaryRepository>();
        services.AddSingleton<InMemoryHealthRecordEntryRepository>();
        services.AddSingleton<ITelemedicineSessionSummaryRepository>(sp =>
            sp.GetRequiredService<InMemoryTelemedicineSessionSummaryRepository>());
        services.AddSingleton<IHealthRecordEntryRepository>(sp =>
            sp.GetRequiredService<InMemoryHealthRecordEntryRepository>());
        if (appointmentConfirmationNotifier is not null)
        {
            services.AddSingleton(appointmentConfirmationNotifier);
        }
        else
        {
            services.AddSingleton<IAppointmentConfirmationNotifier, LoggingAppointmentConfirmationNotifier>();
        }

        if (appointmentRescheduleNotifier is not null)
        {
            services.AddSingleton(appointmentRescheduleNotifier);
        }
        else
        {
            services.AddSingleton<IAppointmentRescheduleNotifier, LoggingAppointmentRescheduleNotifier>();
        }
        services.AddScoped<ILicenseVerificationQueueRepository, LicenseVerificationQueueRepository>();
        services.AddScoped<IDoctorRegistrationWorkflow, DoctorRegistrationWorkflow>();
        services.AddScoped<ILicenseVerificationWorkflow, LicenseVerificationWorkflow>();
        services.AddSingleton<IDoctorLicenseVerificationNotifier, LoggingDoctorLicenseVerificationNotifier>();
        services.AddScoped<IMedicationOrderRepository, MedicationOrderRepository>();
        services.AddSingleton<IPharmacyStockAvailabilityService>(_pharmacyStockAvailability);
        services.AddSingleton<IPharmacyOrderRealtimeNotifier>(_pharmacyOrderRealtimeNotifier);
        services.AddSingleton<IPharmacyOrderReceivedNotifier>(_pharmacyOrderReceivedNotifier);
        services.AddScoped<IPharmacyRepository, PharmacyRepository>();
        services.AddScoped<IPharmacyRegistrationWorkflow, PharmacyRegistrationWorkflow>();
        services.AddScoped<IPharmacyProfileUpdateWorkflow, PharmacyProfileUpdateWorkflow>();
        services.AddScoped<IDoctorProfileUpdateWorkflow, DoctorProfileUpdateWorkflow>();
        services.AddSingleton<ISearchService>(_searchService);
        if (timeProvider is not null)
        {
            services.AddSingleton<TimeProvider>(timeProvider);
            services.AddSingleton(timeProvider);
        }
        else
        {
            services.AddSingleton(TimeProvider.System);
        }

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
