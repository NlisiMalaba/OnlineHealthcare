using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthPlatform.Application;
using HealthPlatform.Application.Auth;
using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Application.PharmacyOrders.Dashboard;
using HealthPlatform.Application.PharmacyOrders.Inventory;
using HealthPlatform.Application.PharmacyOrders.Realtime;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.DrugInteractions;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Application.Identity.UpdateDoctorProfile;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Insurance;
using HealthPlatform.Application.Payments.CreditLine;
using HealthPlatform.Application.Payments.Instalments;
using HealthPlatform.Application.Search;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Storage;
using HealthPlatform.Application.Queue;
using HealthPlatform.Application.Queue.Realtime;
using HealthPlatform.Application.Referrals;
using HealthPlatform.Application.MentalHealth;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Auth;
using HealthPlatform.Infrastructure.Appointments;
using HealthPlatform.Infrastructure.Prescriptions;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.Notifications;
using HealthPlatform.Infrastructure.Notifications.Routing;
using HealthPlatform.Infrastructure.Persistence.Repositories;
using HealthPlatform.Infrastructure.Outbox;
using HealthPlatform.Infrastructure.Persistence;
using HealthPlatform.Application.Audit;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.Realtime;
using HealthPlatform.Infrastructure.Insurance;
using HealthPlatform.Infrastructure.Payments;
using HealthPlatform.Infrastructure.Queue;
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

    private readonly CapturingQueueRealtimeNotifier _queueRealtimeNotifier = new();

    public CapturingQueueRealtimeNotifier QueueRealtimeNotifier => _queueRealtimeNotifier;

    private readonly CapturingQueuePositionNotifier _queuePositionNotifier = new();

    public CapturingQueuePositionNotifier QueuePositionNotifier => _queuePositionNotifier;

    private readonly CapturingQueueStatusNotifier _queueStatusNotifier = new();

    public CapturingQueueStatusNotifier QueueStatusNotifier => _queueStatusNotifier;

    private readonly CapturingQueueDelayNotifier _queueDelayNotifier = new();

    public CapturingQueueDelayNotifier QueueDelayNotifier => _queueDelayNotifier;

    private readonly CapturingPharmacyOrderReceivedNotifier _pharmacyOrderReceivedNotifier = new();

    public CapturingPharmacyOrderReceivedNotifier PharmacyOrderReceivedNotifier => _pharmacyOrderReceivedNotifier;

    private readonly FakePharmacyStockAvailabilityService _pharmacyStockAvailability = new();

    public FakePharmacyStockAvailabilityService PharmacyStockAvailability => _pharmacyStockAvailability;

    private readonly CapturingMedicationOrderPatientNotifier _medicationOrderPatientNotifier = new();

    public CapturingMedicationOrderPatientNotifier MedicationOrderPatientNotifier => _medicationOrderPatientNotifier;

    private readonly CapturingLowStockAlertNotifier _lowStockAlertNotifier = new();

    public CapturingLowStockAlertNotifier LowStockAlertNotifier => _lowStockAlertNotifier;

    private readonly CapturingCreditBalanceWarningNotifier _creditBalanceWarningNotifier = new();

    public CapturingCreditBalanceWarningNotifier CreditBalanceWarningNotifier => _creditBalanceWarningNotifier;

    private readonly CapturingCreditRepaymentReminderNotifier _creditRepaymentReminderNotifier = new();

    public CapturingCreditRepaymentReminderNotifier CreditRepaymentReminderNotifier => _creditRepaymentReminderNotifier;

    private readonly CapturingInstalmentDueReminderNotifier _instalmentDueReminderNotifier = new();

    public CapturingInstalmentDueReminderNotifier InstalmentDueReminderNotifier => _instalmentDueReminderNotifier;

    private readonly CapturingInstalmentMissedPaymentNotifier _instalmentMissedPaymentNotifier = new();

    public CapturingInstalmentMissedPaymentNotifier InstalmentMissedPaymentNotifier => _instalmentMissedPaymentNotifier;

    private readonly CapturingPaymentFailedNotifier _paymentFailedNotifier = new();

    public CapturingPaymentFailedNotifier PaymentFailedNotifier => _paymentFailedNotifier;

    private readonly CapturingMedicationDoseReminderNotifier _medicationDoseReminderNotifier = new();

    public CapturingMedicationDoseReminderNotifier MedicationDoseReminderNotifier => _medicationDoseReminderNotifier;

    private readonly CapturingConsecutiveMissedDosesNextOfKinNotifier _consecutiveMissedDosesNextOfKinNotifier = new();

    public CapturingConsecutiveMissedDosesNextOfKinNotifier ConsecutiveMissedDosesNextOfKinNotifier =>
        _consecutiveMissedDosesNextOfKinNotifier;

    private readonly CapturingNextOfKinDesignationNotifier _nextOfKinDesignationNotifier = new();

    public CapturingNextOfKinDesignationNotifier NextOfKinDesignationNotifier => _nextOfKinDesignationNotifier;

    private readonly CapturingNextOfKinEmergencyAlertNotifier _nextOfKinEmergencyAlertNotifier = new();

    public CapturingNextOfKinEmergencyAlertNotifier NextOfKinEmergencyAlertNotifier =>
        _nextOfKinEmergencyAlertNotifier;

    private readonly ControllableNextOfKinChannelDeliveryGateway _nextOfKinChannelDeliveryGateway = new();

    public ControllableNextOfKinChannelDeliveryGateway NextOfKinChannelDeliveryGateway =>
        _nextOfKinChannelDeliveryGateway;

    private readonly CapturingMedicationScheduleCompletionNotifier _medicationScheduleCompletionNotifier = new();

    public CapturingMedicationScheduleCompletionNotifier MedicationScheduleCompletionNotifier =>
        _medicationScheduleCompletionNotifier;

    private readonly CapturingReferralCreatedNotifier _referralCreatedNotifier = new();

    public CapturingReferralCreatedNotifier ReferralCreatedNotifier => _referralCreatedNotifier;

    private readonly CapturingReferralStatusChangedNotifier _referralStatusChangedNotifier = new();

    public CapturingReferralStatusChangedNotifier ReferralStatusChangedNotifier => _referralStatusChangedNotifier;

    private readonly CapturingReferralTimeoutReminderNotifier _referralTimeoutReminderNotifier = new();

    public CapturingReferralTimeoutReminderNotifier ReferralTimeoutReminderNotifier => _referralTimeoutReminderNotifier;

    public PatientRegistrationTestHost(
        IAppointmentConfirmationNotifier? appointmentConfirmationNotifier = null,
        IAppointmentRescheduleNotifier? appointmentRescheduleNotifier = null,
        IPrescriptionIssuedNotifier? prescriptionIssuedNotifier = null,
        IPrescriptionCancelledNotifier? prescriptionCancelledNotifier = null,
        IDrugInteractionAlertNotifier? drugInteractionAlertNotifier = null,
        IMedicationDoseReminderNotifier? medicationDoseReminderNotifier = null,
        IConsecutiveMissedDosesNextOfKinNotifier? consecutiveMissedDosesNextOfKinNotifier = null,
        INextOfKinEmergencyAlertNotifier? nextOfKinEmergencyAlertNotifier = null,
        INextOfKinChannelDeliveryGateway? nextOfKinChannelDeliveryGateway = null,
        IReferralCreatedNotifier? referralCreatedNotifier = null,
        IReferralStatusChangedNotifier? referralStatusChangedNotifier = null,
        IReferralTimeoutReminderNotifier? referralTimeoutReminderNotifier = null,
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
        services.AddScoped<IHealthRecordAccessRepository, HealthRecordAccessRepository>();
        services.AddScoped<IHealthRecordAccessGuard, HealthRecordAccessGuard>();
        services.AddScoped<IHealthRecordAccessAuditService, HealthRecordAccessAuditService>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddSingleton<IAuditContextAccessor>(new TestAuditContextAccessor());
        services.AddScoped<IHealthRecordProfileChangeRepository, HealthRecordProfileChangeRepository>();
        services.AddScoped<ISocialIdentityVerifier, SocialIdentityVerifier>();
        services.AddScoped<IPatientRegistrationWorkflow, PatientRegistrationWorkflow>();
        services.AddScoped<IPatientProfileUpdateWorkflow, PatientProfileUpdateWorkflow>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        services.AddScoped<IMedicationScheduleRepository, MedicationScheduleRepository>();
        services.AddScoped<IMedicationDoseReminderRepository, MedicationDoseReminderRepository>();
        services.AddScoped<IMedicationDoseReminderDispatcher, MedicationDoseReminderDispatcher>();
        services.AddScoped<IAdherenceEventRepository, AdherenceEventRepository>();
        services.AddScoped<IConsecutiveMissedDoseAlertRepository, ConsecutiveMissedDoseAlertRepository>();
        services.AddScoped<IConsecutiveMissedDoseAlertService, ConsecutiveMissedDoseAlertService>();
        services.AddScoped<IMedicationScheduleCompletionService, MedicationScheduleCompletionService>();
        services.AddSingleton<IMedicationScheduleCompletionNotifier>(_medicationScheduleCompletionNotifier);
        services.AddScoped<INextOfKinRepository, NextOfKinRepository>();
        services.AddSingleton<INextOfKinDesignationNotifier>(_nextOfKinDesignationNotifier);
        if (nextOfKinEmergencyAlertNotifier is not null)
        {
            services.AddSingleton(nextOfKinEmergencyAlertNotifier);
        }
        else
        {
            services.AddSingleton<INextOfKinEmergencyAlertNotifier>(_nextOfKinEmergencyAlertNotifier);
        }

        if (nextOfKinChannelDeliveryGateway is not null)
        {
            services.AddSingleton(nextOfKinChannelDeliveryGateway);
        }
        else
        {
            services.AddSingleton<INextOfKinChannelDeliveryGateway>(_nextOfKinChannelDeliveryGateway);
        }

        services.AddScoped<IEmergencyAlertRepository, EmergencyAlertRepository>();
        services.AddScoped<INextOfKinNotificationDeliveryRepository, NextOfKinNotificationDeliveryRepository>();
        services.AddScoped<INextOfKinEmergencyAlertDeliveryCoordinator, NextOfKinEmergencyAlertDeliveryCoordinator>();
        services.AddScoped<INextOfKinNotificationRetryService, NextOfKinNotificationRetryService>();
        services.AddScoped<IEmergencyAlertDispatchService, EmergencyAlertDispatchService>();
        services.AddScoped<IMissedDoseDetectionDispatcher, MissedDoseDetectionDispatcher>();
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

        if (medicationDoseReminderNotifier is not null)
        {
            services.AddSingleton(medicationDoseReminderNotifier);
        }
        else
        {
            services.AddSingleton<IMedicationDoseReminderNotifier>(_medicationDoseReminderNotifier);
        }

        if (consecutiveMissedDosesNextOfKinNotifier is not null)
        {
            services.AddSingleton(consecutiveMissedDosesNextOfKinNotifier);
        }
        else
        {
            services.AddSingleton<IConsecutiveMissedDosesNextOfKinNotifier>(_consecutiveMissedDosesNextOfKinNotifier);
        }

        services.Configure<RtcOptions>(options => { });
        services.AddSingleton<IRtcProviderResolver, ConfigurableRtcProviderResolver>();
        services.AddSingleton<IRtcTokenService, RtcTokenService>();
        services.AddScoped<ITelemedicineSessionRepository, TelemedicineSessionRepository>();
        services.AddScoped<ITelemedicineSessionParticipantService, TelemedicineSessionParticipantService>();
        services.AddSingleton<ITelemedicineRealtimeNotifier>(_telemedicineRealtimeNotifier);
        services.AddSingleton<InMemoryTelemedicineSessionSummaryRepository>();
        services.AddSingleton<InMemoryTherapySessionSummaryRepository>();
        services.AddSingleton<InMemoryHealthRecordEntryRepository>();
        services.AddSingleton<ITelemedicineSessionSummaryRepository>(sp =>
            sp.GetRequiredService<InMemoryTelemedicineSessionSummaryRepository>());
        services.AddSingleton<ITherapySessionSummaryRepository>(sp =>
            sp.GetRequiredService<InMemoryTherapySessionSummaryRepository>());
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
        services.AddScoped<ILabOrderRepository, LabOrderRepository>();
        services.AddScoped<ILabResultRepository, LabResultRepository>();
        services.AddScoped<IRadiologyReportRepository, RadiologyReportRepository>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IPharmacyDashboardRepository, PharmacyDashboardRepository>();
        services.AddScoped<IQueueEntryRepository, QueueEntryRepository>();
        services.AddScoped<IQueueRealtimeDispatcher, QueueRealtimeDispatcher>();
        services.AddScoped<IReferralRepository, ReferralRepository>();
        services.AddScoped<ITherapySessionRepository, TherapySessionRepository>();
        services.AddSingleton<IPharmacyStockAvailabilityService>(_pharmacyStockAvailability);
        services.AddSingleton<IPharmacyOrderRealtimeNotifier>(_pharmacyOrderRealtimeNotifier);
        services.AddSingleton<IQueueRealtimeNotifier>(_queueRealtimeNotifier);
        services.AddSingleton<IQueuePositionNotifier>(_queuePositionNotifier);
        services.AddSingleton<IQueueStatusNotifier>(_queueStatusNotifier);
        services.AddSingleton<IQueueDelayNotifier>(_queueDelayNotifier);
        services.AddSingleton<IPharmacyOrderReceivedNotifier>(_pharmacyOrderReceivedNotifier);
        services.AddSingleton<IMedicationOrderPatientNotifier>(_medicationOrderPatientNotifier);
        services.AddSingleton<ILowStockAlertNotifier>(_lowStockAlertNotifier);
        services.Configure<HealthPlatform.Infrastructure.PharmacyServices.DeliveryAgentAssignmentOptions>(_ => { });
        services.AddSingleton<IDeliveryAgentAssignmentService, HealthPlatform.Infrastructure.PharmacyServices.ConfigurableDeliveryAgentAssignmentService>();
        services.AddScoped<IPharmacyRepository, PharmacyRepository>();
        services.AddScoped<IPharmacyRegistrationWorkflow, PharmacyRegistrationWorkflow>();
        services.AddScoped<IPharmacyProfileUpdateWorkflow, PharmacyProfileUpdateWorkflow>();
        services.AddScoped<IDoctorProfileUpdateWorkflow, DoctorProfileUpdateWorkflow>();
        services.AddSingleton<ISearchService>(_searchService);
        if (referralCreatedNotifier is not null)
        {
            services.AddSingleton(referralCreatedNotifier);
        }
        else
        {
            services.AddSingleton<IReferralCreatedNotifier>(_referralCreatedNotifier);
        }

        if (referralStatusChangedNotifier is not null)
        {
            services.AddSingleton(referralStatusChangedNotifier);
        }
        else
        {
            services.AddSingleton<IReferralStatusChangedNotifier>(_referralStatusChangedNotifier);
        }

        if (referralTimeoutReminderNotifier is not null)
        {
            services.AddSingleton(referralTimeoutReminderNotifier);
        }
        else
        {
            services.AddSingleton<IReferralTimeoutReminderNotifier>(_referralTimeoutReminderNotifier);
        }

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
        services.AddSingleton<IHealthRecordPdfGenerator, HealthPlatform.Infrastructure.HealthRecords.QuestPdfHealthRecordPdfGenerator>();
        RegisterPaymentGateways(services);
        RegisterInsuranceServices(services);
        RegisterCreditLineServices(services, _creditBalanceWarningNotifier, _creditRepaymentReminderNotifier);
        RegisterInstalmentServices(services, _instalmentDueReminderNotifier, _instalmentMissedPaymentNotifier);
        RegisterPaymentCompletionServices(services, _paymentFailedNotifier);
        services.AddDistributedMemoryCache();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        services.AddSingleton<INotificationPreferenceCache, RedisNotificationPreferenceCache>();
        services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
        services.AddScoped<INotificationPreferenceResolver, StoredNotificationPreferenceResolver>();
        services.AddScoped<IUserRoleResolver, IdentityUserRoleResolver>();
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<INotificationLogWriter, NotificationLogWriter>();
        services.AddScoped<ICriticalNotificationSmsFallbackRepository, CriticalNotificationSmsFallbackRepository>();
        services.AddSingleton<ICriticalNotificationSmsFallbackScheduler, CapturingCriticalNotificationSmsFallbackScheduler>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddScoped<INotificationChannelGatewayResolver, NotificationChannelGatewayResolver>();
        services.AddSingleton<IPushNotificationGateway, Infrastructure.Notifications.Gateways.LoggingPushNotificationGateway>();
        services.AddSingleton<ISmsNotificationGateway, Infrastructure.Notifications.Gateways.LoggingSmsNotificationGateway>();
        services.AddSingleton<IEmailNotificationGateway, Infrastructure.Notifications.Gateways.LoggingEmailNotificationGateway>();
        services.AddScoped<INotificationRecipientResolver, IdentityNotificationRecipientResolver>();
        services.AddScoped<IAppointmentConfirmationNotifier, RoutingAppointmentConfirmationNotifier>();

        _serviceProvider = services.BuildServiceProvider();
        SeedRolesAsync().GetAwaiter().GetResult();
    }

    public ISender Sender => _serviceProvider.GetRequiredService<ISender>();

    public InMemoryHealthRecordEntryRepository HealthRecordEntryRepository =>
        _serviceProvider.GetRequiredService<InMemoryHealthRecordEntryRepository>();

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

    private static void RegisterPaymentCompletionServices(
        IServiceCollection services,
        CapturingPaymentFailedNotifier paymentFailedNotifier)
    {
        services.AddScoped<IPaymentRepository, HealthPlatform.Infrastructure.Persistence.Repositories.PaymentRepository>();
        services.AddSingleton<IPaymentReceiptGenerator, HealthPlatform.Infrastructure.Payments.TextPaymentReceiptGenerator>();
        services.AddScoped<IPaymentCompletionService, PaymentCompletionService>();
        services.AddScoped<IPaymentFailureService, PaymentFailureService>();
        services.AddSingleton<IPaymentFailedNotifier>(paymentFailedNotifier);
    }

    private static void RegisterInstalmentServices(
        IServiceCollection services,
        CapturingInstalmentDueReminderNotifier dueReminderNotifier,
        CapturingInstalmentMissedPaymentNotifier missedPaymentNotifier)
    {
        services.Configure<InstalmentPlanOptions>(_ => { });
        services.AddScoped<IInstalmentPlanRepository, HealthPlatform.Infrastructure.Persistence.Repositories.InstalmentPlanRepository>();
        services.AddScoped<IInstalmentPaymentRepository, HealthPlatform.Infrastructure.Persistence.Repositories.InstalmentPaymentRepository>();
        services.AddScoped<IInstalmentDueReminderDispatcher, InstalmentDueReminderDispatcher>();
        services.AddScoped<IInstalmentMissedPaymentProcessor, InstalmentMissedPaymentProcessor>();
        services.AddSingleton<IInstalmentDueReminderNotifier>(dueReminderNotifier);
        services.AddSingleton<IInstalmentMissedPaymentNotifier>(missedPaymentNotifier);
    }

    private static void RegisterCreditLineServices(
        IServiceCollection services,
        CapturingCreditBalanceWarningNotifier balanceWarningNotifier,
        CapturingCreditRepaymentReminderNotifier repaymentReminderNotifier)
    {
        services.AddScoped<IPatientCreditLineRepository, HealthPlatform.Infrastructure.Persistence.Repositories.PatientCreditLineRepository>();
        services.AddScoped<ICreditLineTransactionRepository, HealthPlatform.Infrastructure.Persistence.Repositories.CreditLineTransactionRepository>();
        services.AddScoped<ICreditRepaymentReminderDispatcher, CreditRepaymentReminderDispatcher>();
        services.AddSingleton<ICreditBalanceWarningNotifier>(balanceWarningNotifier);
        services.AddSingleton<ICreditRepaymentReminderNotifier>(repaymentReminderNotifier);
    }

    private static void RegisterInsuranceServices(IServiceCollection services)
    {
        services.AddSingleton<IHttpClientFactory>(_ => new TestHttpClientFactory());
        services.Configure<InsurerApiOptions>(_ =>
        {
            _.Endpoints =
            [
                new InsurerEndpointOptions { Code = "demo-insurer", Enabled = false }
            ];
        });
        services.AddSingleton<IInsurerApiClientResolver, InsurerApiClientResolver>();
        services.AddScoped<IInsuranceClaimRepository, HealthPlatform.Infrastructure.Persistence.Repositories.InsuranceClaimRepository>();
        services.AddScoped<IPatientInsurancePolicyRepository, HealthPlatform.Infrastructure.Persistence.Repositories.PatientInsurancePolicyRepository>();
        services.AddScoped<IInsuranceClaimStatusPoller, InsuranceClaimStatusPoller>();
        services.AddSingleton<IInsuranceClaimWebhookIdempotencyStore, InMemoryInsuranceClaimWebhookIdempotencyStore>();
        services.AddSingleton<IInsurerApiClient>(sp =>
        {
            var endpoint = new InsurerEndpointOptions { Code = "demo-insurer", Enabled = false };
            return new RestInsurerApiClient(
                endpoint,
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RestInsurerApiClient>>());
        });
    }

    private static void RegisterPaymentGateways(IServiceCollection services)
    {
        services.AddSingleton<IHttpClientFactory>(_ => new TestHttpClientFactory());
        services.Configure<PaymentGatewaysOptions>(_ => { });
        services.AddSingleton<IPaymentGateway, StripePaymentGateway>();
        services.AddSingleton<IPaymentGateway, FlutterwavePaymentGateway>();
        services.AddSingleton<IPaymentGateway, PaystackPaymentGateway>();
        services.AddSingleton<IPaymentGateway, MpesaPaymentGateway>();
        services.AddSingleton<IPaymentGatewayResolver, PaymentGatewayResolver>();
        services.AddSingleton<IPaymentWebhookIdempotencyStore, InMemoryPaymentWebhookIdempotencyStore>();
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "HealthPlatform.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
