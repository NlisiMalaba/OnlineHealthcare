using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Insurance;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Domain.Payments.CreditLine;
using HealthPlatform.Domain.Payments.Instalments;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Domain.Pharmacy;
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

    public DbSet<HealthRecordProfileChange> HealthRecordProfileChanges => Set<HealthRecordProfileChange>();

    public DbSet<Doctor> Doctors => Set<Doctor>();

    public DbSet<DoctorAvailabilitySlot> DoctorAvailabilitySlots => Set<DoctorAvailabilitySlot>();

    public DbSet<LicenseVerificationQueueItem> LicenseVerificationQueue => Set<LicenseVerificationQueueItem>();

    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<TelemedicineSession> TelemedicineSessions => Set<TelemedicineSession>();

    public DbSet<Prescription> Prescriptions => Set<Prescription>();

    public DbSet<MedicationSchedule> MedicationSchedules => Set<MedicationSchedule>();

    public DbSet<MedicationOrder> MedicationOrders => Set<MedicationOrder>();

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();

    public DbSet<PatientInsurancePolicy> PatientInsurancePolicies => Set<PatientInsurancePolicy>();

    public DbSet<PatientCreditLine> PatientCreditLines => Set<PatientCreditLine>();

    public DbSet<CreditLineTransaction> CreditLineTransactions => Set<CreditLineTransaction>();

    public DbSet<InstalmentPlan> InstalmentPlans => Set<InstalmentPlan>();

    public DbSet<InstalmentPayment> InstalmentPayments => Set<InstalmentPayment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
