using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Payments.CreditLine;
using HealthPlatform.Application.Payments.Instalments;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Queue;
using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.Notifications.Gateways;
using HealthPlatform.Infrastructure.Notifications.Routing;
using HealthPlatform.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlatform.Infrastructure.Notifications;

public static class NotificationServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<NotificationChannelsOptions>(
            configuration.GetSection(NotificationChannelsOptions.SectionName));

        services.AddSingleton<IPushNotificationGateway, LoggingPushNotificationGateway>();
        services.AddSingleton<IPushNotificationGateway, FcmPushNotificationGateway>();
        services.AddSingleton<ISmsNotificationGateway, LoggingSmsNotificationGateway>();
        services.AddSingleton<ISmsNotificationGateway, TwilioSmsNotificationGateway>();
        services.AddSingleton<ISmsNotificationGateway, AfricasTalkingSmsNotificationGateway>();
        services.AddSingleton<IEmailNotificationGateway, LoggingEmailNotificationGateway>();
        services.AddSingleton<IEmailNotificationGateway, SendGridEmailNotificationGateway>();
        services.AddSingleton<IEmailNotificationGateway, SesEmailNotificationGateway>();
        services.AddSingleton<INotificationChannelGatewayResolver, NotificationChannelGatewayResolver>();
        services.AddScoped<INotificationRecipientResolver, IdentityNotificationRecipientResolver>();
        services.AddScoped<IUserRoleResolver, IdentityUserRoleResolver>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        services.AddSingleton<INotificationPreferenceCache, RedisNotificationPreferenceCache>();
        services.AddScoped<INotificationPreferenceResolver, StoredNotificationPreferenceResolver>();
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<ICriticalNotificationSmsFallbackRepository, CriticalNotificationSmsFallbackRepository>();
        services.AddScoped<ICriticalNotificationSmsFallbackService, CriticalNotificationSmsFallbackService>();
        services.AddSingleton<ICriticalNotificationSmsFallbackScheduler, HangfireCriticalNotificationSmsFallbackScheduler>();

        services.AddScoped<IAppointmentConfirmationNotifier, RoutingAppointmentConfirmationNotifier>();
        services.AddScoped<IAppointmentReminderNotifier, RoutingAppointmentReminderNotifier>();
        services.AddScoped<IAppointmentRescheduleNotifier, RoutingAppointmentRescheduleNotifier>();
        services.AddScoped<IQueuePositionNotifier, RoutingQueuePositionNotifier>();
        services.AddScoped<IQueueStatusNotifier, RoutingQueueStatusNotifier>();
        services.AddScoped<IQueueDelayNotifier, RoutingQueueDelayNotifier>();
        services.AddScoped<IPrescriptionIssuedNotifier, RoutingPrescriptionIssuedNotifier>();
        services.AddScoped<IPrescriptionCancelledNotifier, RoutingPrescriptionCancelledNotifier>();
        services.AddScoped<IDrugInteractionAlertNotifier, RoutingDrugInteractionAlertNotifier>();
        services.AddScoped<IMedicationDoseReminderNotifier, RoutingMedicationDoseReminderNotifier>();
        services.AddScoped<IMedicationScheduleCompletionNotifier, RoutingMedicationScheduleCompletionNotifier>();
        services.AddScoped<IConsecutiveMissedDosesNextOfKinNotifier, RoutingConsecutiveMissedDosesNextOfKinNotifier>();
        services.AddScoped<IConsecutiveLowMoodPromptNotifier, RoutingConsecutiveLowMoodPromptNotifier>();
        services.AddScoped<IAntenatalRecordCreatedNotifier, RoutingAntenatalRecordCreatedNotifier>();
        services.AddScoped<IAntenatalCheckupReminderNotifier, RoutingAntenatalCheckupReminderNotifier>();
        services.AddScoped<IMedicationOrderPatientNotifier, RoutingMedicationOrderPatientNotifier>();
        services.AddScoped<IPharmacyOrderReceivedNotifier, RoutingPharmacyOrderReceivedNotifier>();
        services.AddScoped<ILowStockAlertNotifier, RoutingLowStockAlertNotifier>();
        services.AddScoped<IAccountLockoutNotifier, RoutingAccountLockoutNotifier>();
        services.AddScoped<IDoctorLicenseVerificationNotifier, RoutingDoctorLicenseVerificationNotifier>();
        services.AddScoped<IPaymentFailedNotifier, RoutingPaymentFailedNotifier>();
        services.AddScoped<ICreditBalanceWarningNotifier, RoutingCreditBalanceWarningNotifier>();
        services.AddScoped<ICreditRepaymentReminderNotifier, RoutingCreditRepaymentReminderNotifier>();
        services.AddScoped<IInstalmentDueReminderNotifier, RoutingInstalmentDueReminderNotifier>();
        services.AddScoped<IInstalmentMissedPaymentNotifier, RoutingInstalmentMissedPaymentNotifier>();
        services.AddScoped<INextOfKinDesignationNotifier, RoutingNextOfKinDesignationNotifier>();
        services.AddScoped<INextOfKinEmergencyAlertNotifier, RoutingNextOfKinEmergencyAlertNotifier>();
        services.AddScoped<INextOfKinChannelDeliveryGateway, RoutingNextOfKinChannelDeliveryGateway>();

        return services;
    }
}
