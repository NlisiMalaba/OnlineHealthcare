using HealthPlatform.Domain.Audit;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Insurance;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Domain.Payments.CreditLine;
using HealthPlatform.Domain.Payments.Instalments;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Domain.Labs;
using HealthPlatform.Domain.MentalHealth;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Domain.Notifications;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Domain.Queue;
using HealthPlatform.Domain.Referrals;
using HealthPlatform.Domain.Maternal;
using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<UserDeviceFingerprint> UserDeviceFingerprints => Set<UserDeviceFingerprint>();

    public DbSet<DeviceLoginVerification> DeviceLoginVerifications => Set<DeviceLoginVerification>();

    public DbSet<DomainEventOutboxEntry> DomainEventOutbox => Set<DomainEventOutboxEntry>();

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();

    public DbSet<HealthRecordAccess> HealthRecordAccesses => Set<HealthRecordAccess>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<HealthRecordProfileChange> HealthRecordProfileChanges => Set<HealthRecordProfileChange>();

    public DbSet<Doctor> Doctors => Set<Doctor>();

    public DbSet<DoctorAvailabilitySlot> DoctorAvailabilitySlots => Set<DoctorAvailabilitySlot>();

    public DbSet<LicenseVerificationQueueItem> LicenseVerificationQueue => Set<LicenseVerificationQueueItem>();

    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<TelemedicineSession> TelemedicineSessions => Set<TelemedicineSession>();

    public DbSet<Prescription> Prescriptions => Set<Prescription>();

    public DbSet<MedicationSchedule> MedicationSchedules => Set<MedicationSchedule>();

    public DbSet<MedicationDoseReminder> MedicationDoseReminders => Set<MedicationDoseReminder>();

    public DbSet<AdherenceEvent> AdherenceEvents => Set<AdherenceEvent>();

    public DbSet<ConsecutiveMissedDoseAlert> ConsecutiveMissedDoseAlerts => Set<ConsecutiveMissedDoseAlert>();

    public DbSet<NextOfKinContact> NextOfKinContacts => Set<NextOfKinContact>();

    public DbSet<EmergencyAlert> EmergencyAlerts => Set<EmergencyAlert>();

    public DbSet<EmergencyAlertContactDelivery> EmergencyAlertContactDeliveries => Set<EmergencyAlertContactDelivery>();

    public DbSet<NextOfKinNotificationDelivery> NextOfKinNotificationDeliveries =>
        Set<NextOfKinNotificationDelivery>();

    public DbSet<MedicationOrder> MedicationOrders => Set<MedicationOrder>();

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();

    public DbSet<LabOrder> LabOrders => Set<LabOrder>();

    public DbSet<LabResult> LabResults => Set<LabResult>();

    public DbSet<RadiologyReport> RadiologyReports => Set<RadiologyReport>();

    public DbSet<PatientInsurancePolicy> PatientInsurancePolicies => Set<PatientInsurancePolicy>();

    public DbSet<PatientCreditLine> PatientCreditLines => Set<PatientCreditLine>();

    public DbSet<CreditLineTransaction> CreditLineTransactions => Set<CreditLineTransaction>();

    public DbSet<InstalmentPlan> InstalmentPlans => Set<InstalmentPlan>();

    public DbSet<InstalmentPayment> InstalmentPayments => Set<InstalmentPayment>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<UserNotificationPreference> UserNotificationPreferences => Set<UserNotificationPreference>();

    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    public DbSet<CriticalNotificationSmsFallback> CriticalNotificationSmsFallbacks =>
        Set<CriticalNotificationSmsFallback>();

    public DbSet<QueueEntry> QueueEntries => Set<QueueEntry>();

    public DbSet<Referral> Referrals => Set<Referral>();

    public DbSet<ReferralHealthRecordAccessGrant> ReferralHealthRecordAccessGrants =>
        Set<ReferralHealthRecordAccessGrant>();

    public DbSet<TherapySession> TherapySessions => Set<TherapySession>();

    public DbSet<MoodChartSharingConsent> MoodChartSharingConsents => Set<MoodChartSharingConsent>();

    public DbSet<ConsecutiveLowMoodPrompt> ConsecutiveLowMoodPrompts => Set<ConsecutiveLowMoodPrompt>();

    public DbSet<AntenatalRecord> AntenatalRecords => Set<AntenatalRecord>();

    public DbSet<AntenatalCheckupScheduleEntry> AntenatalCheckupScheduleEntries =>
        Set<AntenatalCheckupScheduleEntry>();

    public DbSet<BirthPlan> BirthPlans => Set<BirthPlan>();

    public DbSet<MaternalCareAccessGrant> MaternalCareAccessGrants => Set<MaternalCareAccessGrant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
