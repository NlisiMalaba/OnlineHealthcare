using System.Text.Json;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Appointments.Events;
using HealthPlatform.Domain.Events;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Domain.Telemedicine.Events;
using HealthPlatform.Domain.Prescriptions.Events;
using HealthPlatform.Domain.Wellness.Events;
using HealthPlatform.Domain.Insurance.Events;
using HealthPlatform.Domain.Payments.CreditLine.Events;
using HealthPlatform.Domain.Payments.Events;
using HealthPlatform.Domain.Payments.Instalments.Events;
using HealthPlatform.Domain.Pharmacy.Events;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Outbox;

public sealed class OutboxDomainEventDispatcher(
    ApplicationDbContext db,
    IDomainEventPublisher publisher,
    ILogger<OutboxDomainEventDispatcher> logger) : IOutboxDomainEventDispatcher
{
    private const int BatchSize = 50;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<int> DispatchPendingAsync(CancellationToken ct)
    {
        var pending = await db.DomainEventOutbox
            .Where(x => x.ProcessedAtUtc == null)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (pending.Count == 0)
        {
            return 0;
        }

        var dispatched = 0;
        foreach (var row in pending)
        {
            ct.ThrowIfCancellationRequested();
            var domainEvent = Deserialize(row.EventType, row.Payload);
            if (domainEvent is null)
            {
                logger.LogError("Skipping outbox row {OutboxId} with unknown event type {EventType}.", row.Id, row.EventType);
                row.ProcessedAtUtc = DateTime.UtcNow;
                continue;
            }

            await publisher.PublishAsync(domainEvent, ct);
            row.ProcessedAtUtc = DateTime.UtcNow;
            dispatched++;
        }

        await db.SaveChangesAsync(ct);
        return dispatched;
    }

    private static IDomainEvent? Deserialize(string eventType, string payload) =>
        eventType switch
        {
            "HealthPlatform.Domain.Identity.Events.AccountLockedDomainEvent" =>
                JsonSerializer.Deserialize<AccountLockedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Identity.Events.PatientRegisteredDomainEvent" =>
                JsonSerializer.Deserialize<PatientRegisteredDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Identity.Events.DoctorRegisteredDomainEvent" =>
                JsonSerializer.Deserialize<DoctorRegisteredDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Identity.Events.DoctorLicenseVerifiedDomainEvent" =>
                JsonSerializer.Deserialize<DoctorLicenseVerifiedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Identity.Events.DoctorLicenseRejectedDomainEvent" =>
                JsonSerializer.Deserialize<DoctorLicenseRejectedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Identity.Events.DoctorAvailabilityChangedDomainEvent" =>
                JsonSerializer.Deserialize<DoctorAvailabilityChangedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Identity.Events.DoctorProfileUpdatedDomainEvent" =>
                JsonSerializer.Deserialize<DoctorProfileUpdatedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Appointments.Events.PaymentCompletedDomainEvent" =>
                JsonSerializer.Deserialize<PaymentCompletedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Payments.Events.PaymentFailedDomainEvent" =>
                JsonSerializer.Deserialize<PaymentFailedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Appointments.Events.AppointmentConfirmedDomainEvent" =>
                JsonSerializer.Deserialize<AppointmentConfirmedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Appointments.Events.AppointmentCancelledDomainEvent" =>
                JsonSerializer.Deserialize<AppointmentCancelledDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Appointments.Events.AppointmentRefundRequestedDomainEvent" =>
                JsonSerializer.Deserialize<AppointmentRefundRequestedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Appointments.Events.AppointmentLateCancellationPolicyAppliedDomainEvent" =>
                JsonSerializer.Deserialize<AppointmentLateCancellationPolicyAppliedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Appointments.Events.AppointmentRescheduledDomainEvent" =>
                JsonSerializer.Deserialize<AppointmentRescheduledDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Identity.Events.PharmacyRegisteredDomainEvent" =>
                JsonSerializer.Deserialize<PharmacyRegisteredDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Identity.Events.PharmacyProfileUpdatedDomainEvent" =>
                JsonSerializer.Deserialize<PharmacyProfileUpdatedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Identity.Events.PharmacyStockChangedDomainEvent" =>
                JsonSerializer.Deserialize<PharmacyStockChangedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Telemedicine.Events.TelemedicineSessionEndedDomainEvent" =>
                JsonSerializer.Deserialize<TelemedicineSessionEndedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Prescriptions.Events.PrescriptionIssuedDomainEvent" =>
                JsonSerializer.Deserialize<PrescriptionIssuedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Prescriptions.Events.PrescriptionCancelledDomainEvent" =>
                JsonSerializer.Deserialize<PrescriptionCancelledDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Prescriptions.Events.PrescriptionDispensedDomainEvent" =>
                JsonSerializer.Deserialize<PrescriptionDispensedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Prescriptions.Events.DrugInteractionAlertDetectedDomainEvent" =>
                JsonSerializer.Deserialize<DrugInteractionAlertDetectedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Pharmacy.Events.MedicationOrderPlacedDomainEvent" =>
                JsonSerializer.Deserialize<MedicationOrderPlacedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Pharmacy.Events.OrderStatusChangedDomainEvent" =>
                JsonSerializer.Deserialize<OrderStatusChangedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Pharmacy.Events.InventoryLowStockDetectedDomainEvent" =>
                JsonSerializer.Deserialize<InventoryLowStockDetectedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Insurance.Events.InsuranceClaimSubmittedDomainEvent" =>
                JsonSerializer.Deserialize<InsuranceClaimSubmittedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Insurance.Events.InsuranceClaimStatusChangedDomainEvent" =>
                JsonSerializer.Deserialize<InsuranceClaimStatusChangedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Payments.CreditLine.Events.CreditLineChargedDomainEvent" =>
                JsonSerializer.Deserialize<CreditLineChargedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Payments.CreditLine.Events.CreditBalanceWarningTriggeredDomainEvent" =>
                JsonSerializer.Deserialize<CreditBalanceWarningTriggeredDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Payments.Instalments.Events.InstalmentPlanCreatedDomainEvent" =>
                JsonSerializer.Deserialize<InstalmentPlanCreatedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Payments.Instalments.Events.InstalmentPaymentMissedDomainEvent" =>
                JsonSerializer.Deserialize<InstalmentPaymentMissedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Wellness.Events.ConsecutiveMissedDosesDetectedDomainEvent" =>
                JsonSerializer.Deserialize<ConsecutiveMissedDosesDetectedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Wellness.Events.MedicationScheduleCompletedDomainEvent" =>
                JsonSerializer.Deserialize<MedicationScheduleCompletedDomainEvent>(payload, SerializerOptions),
            _ => null
        };
}
