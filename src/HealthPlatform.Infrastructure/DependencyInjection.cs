using HealthPlatform.Application.Audit;
using HealthPlatform.Application.Auth;
using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.MentalHealth;
using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Application.MentalHealth.CrisisProtocol;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Application.PharmacyOrders.Dashboard;
using HealthPlatform.Application.PharmacyOrders.Inventory;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.DrugInteractions;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Application.Storage;
using HealthPlatform.Infrastructure.Storage;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Search;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Queue;
using HealthPlatform.Application.Queue.Realtime;
using HealthPlatform.Application.Referrals;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Maternal.BirthPlans;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Maternal.GrowthEntries;
using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Infrastructure.Audit;
using HealthPlatform.Infrastructure.Auth;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Infrastructure.Appointments;
using HealthPlatform.Infrastructure.Telemedicine;
using HealthPlatform.Infrastructure.Wellness;
using HealthPlatform.Infrastructure.PharmacyServices;
using HealthPlatform.Infrastructure.Prescriptions;
using HealthPlatform.Infrastructure.Queue;
using HealthPlatform.Infrastructure.Labs;
using HealthPlatform.Infrastructure.HealthRecords;
using HealthPlatform.Infrastructure.Hosting;
using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.MongoDb;
using HealthPlatform.Infrastructure.NextOfKin;
using HealthPlatform.Infrastructure.Notifications;
using HealthPlatform.Infrastructure.Insurance;
using HealthPlatform.Infrastructure.Jobs;
using HealthPlatform.Infrastructure.Outbox;
using HealthPlatform.Infrastructure.Payments.CreditLine;
using HealthPlatform.Infrastructure.Payments.Instalments;
using HealthPlatform.Infrastructure.Payments;
using HealthPlatform.Infrastructure.Persistence;
using HealthPlatform.Infrastructure.Persistence.Repositories;
using HealthPlatform.Infrastructure.Search;
using HealthPlatform.Infrastructure.Security;
using HealthPlatform.Infrastructure.Referrals;
using HealthPlatform.Infrastructure.Maternal;
using HealthPlatform.Infrastructure.Vaccinations;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace HealthPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Aes256AtRestEncryptionOptions>(
            configuration.GetSection(Aes256AtRestEncryptionOptions.SectionName));
        services.AddSingleton<IAtRestEncryption, Aes256AtRestEncryption>();
        services.AddPaymentGateways(configuration);
        services.AddInsuranceServices(configuration);
        services.AddCreditLineServices();
        services.AddInstalmentServices(configuration);
        services.AddPaymentCompletionServices();
        services.AddScoped<IOutboxDomainEventDispatcher, OutboxDomainEventDispatcher>();
        services.AddHealthPlatformMongoDb(configuration);
        services.AddHealthRecordServices();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IAccountLockoutService, AccountLockoutService>();
        services.AddNotificationServices(configuration);
        services.AddSingleton(TimeProvider.System);

        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redis);
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redis));
            services.AddSingleton<ISlotHoldService, RedisSlotHoldService>();
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<ISlotHoldService, InMemorySlotHoldService>();
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
        services.AddScoped<IHealthRecordAccessRepository, HealthRecordAccessRepository>();
        services.AddScoped<IHealthRecordAccessGuard, HealthRecordAccessGuard>();
        services.AddScoped<IHealthRecordAccessAuditService, HealthRecordAccessAuditService>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditContextAccessor, HttpAuditContextAccessor>();
        services.AddScoped<IPatientRegistrationWorkflow, PatientRegistrationWorkflow>();
        services.AddScoped<IPatientProfileUpdateWorkflow, PatientProfileUpdateWorkflow>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.Configure<RtcOptions>(configuration.GetSection(RtcOptions.SectionName));
        services.AddSingleton<IRtcProviderResolver, ConfigurableRtcProviderResolver>();
        services.AddSingleton<IRtcTokenService, RtcTokenService>();
        services.AddScoped<ITelemedicineSessionRepository, TelemedicineSessionRepository>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        services.AddScoped<IMedicationScheduleRepository, MedicationScheduleRepository>();
        services.AddScoped<IMedicationDoseReminderRepository, MedicationDoseReminderRepository>();
        services.AddScoped<IMedicationDoseReminderDispatcher, MedicationDoseReminderDispatcher>();
        services.AddScoped<IAdherenceEventRepository, AdherenceEventRepository>();
        services.AddScoped<IConsecutiveMissedDoseAlertRepository, ConsecutiveMissedDoseAlertRepository>();
        services.AddScoped<IConsecutiveMissedDoseAlertService, ConsecutiveMissedDoseAlertService>();
        services.AddScoped<IMedicationScheduleCompletionService, MedicationScheduleCompletionService>();
        services.AddScoped<INextOfKinRepository, NextOfKinRepository>();
        services.AddScoped<IEmergencyAlertRepository, EmergencyAlertRepository>();
        services.AddScoped<IEmergencyAlertDispatchService, EmergencyAlertDispatchService>();
        services.AddScoped<INextOfKinEmergencyAlertDeliveryCoordinator, NextOfKinEmergencyAlertDeliveryCoordinator>();
        services.AddScoped<INextOfKinNotificationDeliveryRepository, NextOfKinNotificationDeliveryRepository>();
        services.AddScoped<INextOfKinNotificationRetryService, NextOfKinNotificationRetryService>();
        services.AddScoped<IMissedDoseDetectionDispatcher, MissedDoseDetectionDispatcher>();
        services.AddScoped<IMedicationOrderRepository, MedicationOrderRepository>();
        services.AddScoped<ILabOrderRepository, LabOrderRepository>();
        services.AddScoped<ILabResultRepository, LabResultRepository>();
        services.AddScoped<IRadiologyReportRepository, RadiologyReportRepository>();
        services.AddScoped<ILabPartnerOrderClient, LoggingLabPartnerOrderClient>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IPharmacyDashboardRepository, PharmacyDashboardRepository>();
        services.AddScoped<IQueueEntryRepository, QueueEntryRepository>();
        services.AddScoped<IQueueRealtimeDispatcher, QueueRealtimeDispatcher>();
        services.AddScoped<IReferralRepository, ReferralRepository>();
        services.AddScoped<IAntenatalRecordRepository, AntenatalRecordRepository>();
        services.AddScoped<IBirthPlanRepository, BirthPlanRepository>();
        services.AddScoped<IMaternalCareAccessRepository, MaternalCareAccessRepository>();
        services.AddScoped<IMaternalCareAccessGuard, MaternalCareAccessGuard>();
        services.AddScoped<IChildProfileRepository, ChildProfileRepository>();
        services.AddScoped<IVaccinationScheduleRepository, VaccinationScheduleRepository>();
        services.AddScoped<IVaccinationRecordRepository, VaccinationRecordRepository>();
        services.AddScoped<IVaccinationScheduleInitializer, VaccinationScheduleInitializer>();
        services.AddScoped<IVaccinationReminderDispatcher, VaccinationReminderDispatcher>();
        services.AddScoped<IGrowthEntryRepository, GrowthEntryRepository>();
        services.AddScoped<IGrowthEntryRepository, GrowthEntryRepository>();
        services.AddScoped<ITherapySessionRepository, TherapySessionRepository>();
        services.AddScoped<IMoodChartSharingConsentRepository, MoodChartSharingConsentRepository>();
        services.AddScoped<IConsecutiveLowMoodPromptRepository, ConsecutiveLowMoodPromptRepository>();
        services.AddScoped<IConsecutiveLowMoodPromptService, ConsecutiveLowMoodPromptService>();
        services.AddSingleton<ICrisisKeywordDetector, CrisisKeywordDetector>();
        services.AddScoped<ICrisisProtocolService, CrisisProtocolService>();
        services.AddScoped<IReferralTimeoutReminderDispatcher, ReferralTimeoutReminderDispatcher>();
        services.AddScoped<IAntenatalCheckupReminderDispatcher, AntenatalCheckupReminderDispatcher>();
        services.AddScoped<IFetalMonitoringReminderDispatcher, FetalMonitoringReminderDispatcher>();
        services.AddSingleton<IPharmacyOrderReceivedNotifier, LoggingPharmacyOrderReceivedNotifier>();
        services.AddSingleton<ILowStockAlertNotifier, LoggingLowStockAlertNotifier>();
        services.AddSingleton<IMedicationOrderPatientNotifier, LoggingMedicationOrderPatientNotifier>();
        services.Configure<DeliveryAgentAssignmentOptions>(
            configuration.GetSection(DeliveryAgentAssignmentOptions.SectionName));
        services.AddSingleton<IDeliveryAgentAssignmentService, ConfigurableDeliveryAgentAssignmentService>();
        services.AddSingleton<IDrugInteractionChecker, StaticDrugInteractionChecker>();
        services.AddSingleton<IPrescriptionIssuedNotifier, LoggingPrescriptionIssuedNotifier>();
        services.AddSingleton<IPrescriptionCancelledNotifier, LoggingPrescriptionCancelledNotifier>();
        services.AddSingleton<IDrugInteractionAlertNotifier, LoggingDrugInteractionAlertNotifier>();
        services.AddSingleton<IAppointmentConfirmationNotifier, LoggingAppointmentConfirmationNotifier>();
        services.AddSingleton<IAppointmentRescheduleNotifier, LoggingAppointmentRescheduleNotifier>();
        services.AddSingleton<IAppointmentReminderNotifier, LoggingAppointmentReminderNotifier>();
        services.AddSingleton<IQueuePositionNotifier, LoggingQueuePositionNotifier>();
        services.AddSingleton<IQueueStatusNotifier, LoggingQueueStatusNotifier>();
        services.AddSingleton<IQueueDelayNotifier, LoggingQueueDelayNotifier>();
        services.AddSingleton<IReferralCreatedNotifier, LoggingReferralCreatedNotifier>();
        services.AddSingleton<IReferralStatusChangedNotifier, LoggingReferralStatusChangedNotifier>();
        services.AddSingleton<IReferralTimeoutReminderNotifier, LoggingReferralTimeoutReminderNotifier>();
        services.AddSingleton<IAntenatalRecordCreatedNotifier, LoggingAntenatalRecordCreatedNotifier>();
        services.AddSingleton<IAntenatalCheckupReminderNotifier, LoggingAntenatalCheckupReminderNotifier>();
        services.AddSingleton<IFetalMonitoringReminderNotifier, LoggingFetalMonitoringReminderNotifier>();
        services.AddSingleton<IBirthPlanUpdatedNotifier, LoggingBirthPlanUpdatedNotifier>();
        services.AddSingleton<IVaccinationReminderNotifier, LoggingVaccinationReminderNotifier>();
        services.AddSingleton<IChildGrowthOutOfRangeNotifier, LoggingChildGrowthOutOfRangeNotifier>();
        services.AddSingleton<IChildGrowthOutOfRangeNotifier, LoggingChildGrowthOutOfRangeNotifier>();
        services.AddScoped<IAppointmentReminderDispatcher, AppointmentReminderDispatcher>();
        services.AddScoped<ILicenseVerificationQueueRepository, LicenseVerificationQueueRepository>();
        services.AddScoped<IDoctorRegistrationWorkflow, DoctorRegistrationWorkflow>();
        services.AddScoped<ILicenseVerificationWorkflow, LicenseVerificationWorkflow>();
        services.AddScoped<IDoctorProfileUpdateWorkflow, DoctorProfileUpdateWorkflow>();
        services.AddScoped<IPharmacyRepository, PharmacyRepository>();
        services.AddScoped<IPharmacyRegistrationWorkflow, PharmacyRegistrationWorkflow>();
        services.AddScoped<IPharmacyProfileUpdateWorkflow, PharmacyProfileUpdateWorkflow>();
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.SectionName));

        var elasticsearchUri = configuration["Elasticsearch:Uri"];
        if (!string.IsNullOrWhiteSpace(elasticsearchUri))
        {
            var elasticsearchSettings = new ElasticsearchClientSettings(new Uri(elasticsearchUri));
            services.AddSingleton(new ElasticsearchClient(elasticsearchSettings));
            services.AddSingleton<IElasticsearchIndexManager, ElasticsearchIndexManager>();
            services.AddHostedService<ElasticsearchIndexInitializerHostedService>();
            services.AddScoped<ISearchService, ElasticsearchSearchService>();
            services.AddScoped<DoctorElasticsearchSearcher>();
            services.AddScoped<PharmacyElasticsearchSearcher>();
            services.AddScoped<LabPartnerElasticsearchSearcher>();
            services.AddScoped<IPharmacyStockAvailabilityService, PharmacyStockAvailabilityService>();
        }
        else
        {
            services.AddSingleton<ISearchService, LoggingSearchService>();
            services.AddSingleton<IPharmacyStockAvailabilityService, LoggingPharmacyStockAvailabilityService>();
        }
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
        services.AddTransient<InsuranceClaimStatusPollingJob>();
        services.AddTransient<CreditRepaymentReminderJob>();
        services.AddTransient<InstalmentDueReminderJob>();
        services.AddTransient<InstalmentMissedPaymentJob>();
        services.AddTransient<NextOfKinNotificationRetryJob>();
        services.AddTransient<CriticalNotificationSmsFallbackJob>();
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
