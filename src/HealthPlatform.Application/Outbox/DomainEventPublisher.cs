using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Prescriptions.Notifications;
using HealthPlatform.Application.Payments.CreditLine.Notifications;
using HealthPlatform.Application.PharmacyOrders.Notifications;
using HealthPlatform.Application.Search.Notifications;
using HealthPlatform.Application.Telemedicine.Notifications;
using HealthPlatform.Domain.Appointments.Events;
using HealthPlatform.Domain.Events;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Domain.Insurance.Events;
using HealthPlatform.Domain.Telemedicine.Events;
using HealthPlatform.Domain.Prescriptions.Events;
using HealthPlatform.Domain.Payments.CreditLine.Events;
using HealthPlatform.Domain.Pharmacy.Events;
using MediatR;

namespace HealthPlatform.Application.Outbox;

public sealed class DomainEventPublisher(IMediator mediator) : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct) =>
        domainEvent switch
        {
            AccountLockedDomainEvent e => mediator.Publish(
                new AccountLockedNotification(e.UserId, e.LockoutEndUtc, e.FailedAttemptCount),
                ct),
            PatientRegisteredDomainEvent e => mediator.Publish(
                new PatientRegisteredNotification(e.PatientId, e.OccurredAtUtc),
                ct),
            DoctorRegisteredDomainEvent e => mediator.Publish(
                new DoctorRegisteredNotification(
                    e.DoctorId,
                    e.LicenseNumber,
                    e.FullName,
                    e.OccurredAtUtc),
                ct),
            DoctorLicenseVerifiedDomainEvent e => mediator.Publish(
                new DoctorLicenseVerifiedNotification(
                    e.DoctorId,
                    e.UserId,
                    e.FullName,
                    e.OccurredAtUtc),
                ct),
            DoctorLicenseRejectedDomainEvent e => mediator.Publish(
                new DoctorLicenseRejectedNotification(
                    e.DoctorId,
                    e.UserId,
                    e.FullName,
                    e.Reason,
                    e.OccurredAtUtc),
                ct),
            DoctorAvailabilityChangedDomainEvent e => mediator.Publish(
                new DoctorAvailabilityChangedNotification(e.DoctorId, e.OccurredAtUtc),
                ct),
            PaymentCompletedDomainEvent e => mediator.Publish(
                new PaymentCompletedNotification(e.AppointmentId, e.PaymentId, e.OccurredAtUtc),
                ct),
            AppointmentConfirmedDomainEvent e => mediator.Publish(
                new AppointmentConfirmedNotification(
                    e.AppointmentId,
                    e.PatientId,
                    e.DoctorId,
                    e.ScheduledAtUtc,
                    e.ConfirmedAtUtc,
                    e.OccurredAtUtc),
                ct),
            AppointmentRescheduledDomainEvent e => mediator.Publish(
                new AppointmentRescheduledNotification(
                    e.AppointmentId,
                    e.PatientId,
                    e.DoctorId,
                    e.PreviousScheduledAtUtc,
                    e.NewScheduledAtUtc,
                    e.OccurredAtUtc),
                ct),
            AppointmentRefundRequestedDomainEvent => Task.CompletedTask,
            AppointmentLateCancellationPolicyAppliedDomainEvent => Task.CompletedTask,
            AppointmentCancelledDomainEvent => Task.CompletedTask,
            DoctorProfileUpdatedDomainEvent e => mediator.Publish(
                new DoctorProfileUpdatedNotification(e.DoctorId, e.OccurredAtUtc),
                ct),
            PharmacyRegisteredDomainEvent e => mediator.Publish(
                new PharmacyRegisteredSearchNotification(e.PharmacyId, e.OccurredAtUtc),
                ct),
            PharmacyProfileUpdatedDomainEvent e => mediator.Publish(
                new PharmacyProfileUpdatedNotification(e.PharmacyId, e.OccurredAtUtc),
                ct),
            PharmacyStockChangedDomainEvent e => mediator.Publish(
                new PharmacyStockChangedNotification(e.PharmacyId, e.StockSummary, e.OccurredAtUtc),
                ct),
            TelemedicineSessionEndedDomainEvent e => mediator.Publish(
                new TelemedicineSessionEndedNotification(
                    e.SessionId,
                    e.AppointmentId,
                    e.PatientId,
                    e.DoctorId,
                    e.Mode,
                    e.DurationSeconds,
                    e.StartedAtUtc,
                    e.EndedAtUtc,
                    e.RecordingEnabled,
                    e.OccurredAtUtc),
                ct),
            PrescriptionIssuedDomainEvent e => mediator.Publish(
                new PrescriptionIssuedNotification(
                    e.PrescriptionId,
                    e.DoctorId,
                    e.PatientId,
                    e.HealthRecordId,
                    e.IssuedAtUtc,
                    e.ExpiresAtUtc,
                    e.OccurredAtUtc),
                ct),
            PrescriptionCancelledDomainEvent e => mediator.Publish(
                new PrescriptionCancelledNotification(
                    e.PrescriptionId,
                    e.DoctorId,
                    e.PatientId,
                    e.CancelledAtUtc,
                    e.OccurredAtUtc),
                ct),
            DrugInteractionAlertDetectedDomainEvent e => mediator.Publish(
                new DrugInteractionAlertDetectedNotification(
                    e.DoctorId,
                    e.PatientId,
                    e.ProposedMedicationName,
                    e.InteractingMedicationName,
                    e.InteractionDescription,
                    e.OccurredAtUtc),
                ct),
            MedicationOrderPlacedDomainEvent e => mediator.Publish(
                new MedicationOrderPlacedNotification(
                    e.OrderId,
                    e.PatientId,
                    e.PharmacyId,
                    e.PrescriptionId,
                    e.MedicationSku,
                    e.MedicationName,
                    e.Dosage,
                    e.Frequency,
                    e.DurationDays,
                    e.SpecialInstructions,
                    e.DeliveryType,
                    e.DeliveryAddress,
                    e.OccurredAtUtc),
                ct),
            OrderStatusChangedDomainEvent e => mediator.Publish(
                new OrderStatusChangedNotification(
                    e.OrderId,
                    e.PatientId,
                    e.PharmacyId,
                    e.MedicationSku,
                    e.PreviousStatus,
                    e.NewStatus,
                    e.DeliveryType,
                    e.TrackingUrl,
                    e.DeliveryAgentName,
                    e.RejectionReason,
                    e.ClarificationMessage,
                    e.OccurredAtUtc),
                ct),
            InventoryLowStockDetectedDomainEvent e => mediator.Publish(
                new InventoryLowStockDetectedNotification(
                    e.InventoryItemId,
                    e.PharmacyId,
                    e.MedicationSku,
                    e.MedicationName,
                    e.Quantity,
                    e.LowStockThreshold,
                    e.OccurredAtUtc),
                ct),
            CreditBalanceWarningTriggeredDomainEvent e => mediator.Publish(
                new CreditBalanceWarningNotification(
                    e.CreditLineId,
                    e.PatientId,
                    e.OutstandingBalanceMinorUnits,
                    e.CreditLimitMinorUnits,
                    e.Currency,
                    e.OccurredAtUtc),
                ct),
            CreditLineChargedDomainEvent => Task.CompletedTask,
            _ => Task.CompletedTask
        };
}
