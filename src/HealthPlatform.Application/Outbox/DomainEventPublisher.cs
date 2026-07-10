using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Prescriptions.Notifications;
using HealthPlatform.Application.Payments.CreditLine.Notifications;
using HealthPlatform.Application.Payments.Instalments.Notifications;
using HealthPlatform.Application.Payments.Notifications;
using HealthPlatform.Application.PharmacyOrders.Notifications;
using HealthPlatform.Application.Search.Notifications;
using HealthPlatform.Application.Telemedicine.Notifications;
using HealthPlatform.Application.Wellness.Notifications;
using HealthPlatform.Application.MentalHealth.MoodLogs.Notifications;
using HealthPlatform.Application.Referrals.Notifications;
using HealthPlatform.Application.Maternal.AntenatalRecords.Notifications;
using HealthPlatform.Application.Maternal.BirthPlans.Notifications;
using HealthPlatform.Domain.Appointments.Events;
using HealthPlatform.Domain.Events;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Domain.Insurance.Events;
using HealthPlatform.Domain.Telemedicine.Events;
using HealthPlatform.Domain.Prescriptions.Events;
using HealthPlatform.Domain.Wellness.Events;
using HealthPlatform.Domain.Payments.CreditLine.Events;
using HealthPlatform.Domain.Payments.Events;
using HealthPlatform.Domain.Payments.Instalments.Events;
using HealthPlatform.Domain.Pharmacy.Events;
using HealthPlatform.Domain.MentalHealth.Events;
using HealthPlatform.Domain.Referrals.Events;
using HealthPlatform.Domain.Maternal.Events;
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
            PaymentFailedDomainEvent e => mediator.Publish(
                new PaymentFailedNotification(
                    e.PaymentId,
                    e.PatientId,
                    e.AppointmentId,
                    e.MedicationOrderId,
                    e.LabOrderId,
                    e.FailureCode,
                    e.FailureMessage,
                    e.RetentionExpiresAtUtc,
                    e.OccurredAtUtc),
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
            PrescriptionDispensedDomainEvent e => mediator.Publish(
                new PrescriptionDispensedNotification(
                    e.PrescriptionId,
                    e.PatientId,
                    e.MedicationName,
                    e.Dosage,
                    e.Frequency,
                    e.DurationDays,
                    e.DispensedAtUtc,
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
            InstalmentPlanCreatedDomainEvent => Task.CompletedTask,
            InstalmentPaymentMissedDomainEvent e => mediator.Publish(
                new InstalmentPaymentMissedNotification(
                    e.InstalmentPaymentId,
                    e.InstalmentPlanId,
                    e.PatientId,
                    e.SequenceNumber,
                    e.AmountMinorUnits,
                    e.LateFeeMinorUnits,
                    e.Currency,
                    e.DueDate,
                    e.OccurredAtUtc),
                ct),
            ConsecutiveMissedDosesDetectedDomainEvent e => mediator.Publish(
                new ConsecutiveMissedDosesDetectedNotification(
                    e.PatientId,
                    e.TriggeringAdherenceEventId,
                    e.StreakEndScheduledAtUtc,
                    e.OccurredAtUtc),
                ct),
            ConsecutiveLowMoodDetectedDomainEvent e => mediator.Publish(
                new ConsecutiveLowMoodDetectedNotification(
                    e.PatientId,
                    e.TriggeringMoodLogId,
                    e.StreakEndLoggedAtUtc,
                    e.OccurredAtUtc),
                ct),
            MedicationScheduleCompletedDomainEvent e => mediator.Publish(
                new MedicationScheduleCompletedNotification(
                    e.ScheduleId,
                    e.PrescriptionId,
                    e.PatientId,
                    e.MedicationName,
                    e.CompletedAtUtc),
                ct),
            ReferralCreatedDomainEvent e => mediator.Publish(
                new ReferralCreatedNotification(
                    e.ReferralId,
                    e.PatientId,
                    e.ReferringDoctorId,
                    e.ReceivingDoctorId,
                    e.Reason,
                    e.PatientConsentAtUtc,
                    e.CreatedAtUtc,
                    e.OccurredAtUtc),
                ct),
            ReferralStatusChangedDomainEvent e => mediator.Publish(
                new ReferralStatusChangedNotification(
                    e.ReferralId,
                    e.PatientId,
                    e.ReferringDoctorId,
                    e.ReceivingDoctorId,
                    e.Status.ToString().ToLowerInvariant(),
                    e.Reason,
                    e.RespondedAtUtc,
                    e.OccurredAtUtc),
                ct),
            AntenatalRecordCreatedDomainEvent e => mediator.Publish(
                new AntenatalRecordCreatedNotification(
                    e.AntenatalRecordId,
                    e.PatientId,
                    e.ObstetricDoctorId,
                    e.EstimatedDueDate,
                    e.GestationalAgeWeeks,
                    e.CreatedAtUtc,
                    e.OccurredAtUtc),
                ct),
            BirthPlanUpdatedDomainEvent e => mediator.Publish(
                new BirthPlanUpdatedNotification(
                    e.BirthPlanId,
                    e.AntenatalRecordId,
                    e.PatientId,
                    e.ObstetricDoctorId,
                    e.UpdatedAtUtc,
                    e.OccurredAtUtc),
                ct),
            _ => Task.CompletedTask
        };
}
