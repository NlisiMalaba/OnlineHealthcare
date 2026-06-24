using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Appointments.Events;

namespace HealthPlatform.Domain.Appointments;

public sealed class Appointment : Entity
{
    private Appointment()
    {
    }

    public Guid PatientId { get; private set; }

    public Guid DoctorId { get; private set; }

    public Guid SlotId { get; private set; }

    public DateTime ScheduledAtUtc { get; private set; }

    public AppointmentStatus Status { get; private set; }

    public DateTime SlotHoldExpiresAtUtc { get; private set; }

    public DateTime? ReminderSentAtUtc { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public string? CancellationReason { get; private set; }

    public static Appointment CreatePendingPayment(
        Guid patientId,
        Guid doctorId,
        Guid slotId,
        DateTime scheduledAtUtc,
        DateTime slotHoldExpiresAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("Doctor id is required.", nameof(doctorId));
        }

        if (slotId == Guid.Empty)
        {
            throw new ArgumentException("Slot id is required.", nameof(slotId));
        }

        if (scheduledAtUtc == default)
        {
            throw new ArgumentException("Scheduled time is required.", nameof(scheduledAtUtc));
        }

        if (scheduledAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Scheduled time must be UTC.", nameof(scheduledAtUtc));
        }

        if (slotHoldExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new ArgumentException("Slot hold expiration must be in the future.", nameof(slotHoldExpiresAtUtc));
        }

        return new Appointment
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            DoctorId = doctorId,
            SlotId = slotId,
            ScheduledAtUtc = scheduledAtUtc,
            Status = AppointmentStatus.PendingPayment,
            SlotHoldExpiresAtUtc = slotHoldExpiresAtUtc
        };
    }

    public void ConfirmOnPayment(DateTime confirmedAtUtc)
    {
        if (Status == AppointmentStatus.Confirmed)
        {
            return;
        }

        if (Status == AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot confirm a cancelled appointment.");
        }

        Status = AppointmentStatus.Confirmed;
        Touch();
        RaiseDomainEvent(new AppointmentConfirmedDomainEvent(
            Id,
            PatientId,
            DoctorId,
            ScheduledAtUtc,
            confirmedAtUtc));
    }

    public bool MarkReminderSent(DateTime sentAtUtc)
    {
        if (Status != AppointmentStatus.Confirmed)
        {
            return false;
        }

        if (ReminderSentAtUtc.HasValue)
        {
            return false;
        }

        ReminderSentAtUtc = sentAtUtc;
        Touch();
        return true;
    }

    public AppointmentCancellationOutcome Cancel(
        DateTime cancelledAtUtc,
        TimeSpan earlyCancellationWindow,
        decimal doctorLateCancellationRetentionPercent,
        string? reason)
    {
        if (Status != AppointmentStatus.Confirmed)
        {
            throw new AppointmentNotCancellableException(Status);
        }

        if (cancelledAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Cancellation time must be UTC.", nameof(cancelledAtUtc));
        }

        if (doctorLateCancellationRetentionPercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(doctorLateCancellationRetentionPercent));
        }

        var timeUntilScheduled = ScheduledAtUtc - cancelledAtUtc;
        var isEarlyCancellation = timeUntilScheduled > earlyCancellationWindow;

        Status = AppointmentStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        Touch();

        RaiseDomainEvent(new AppointmentCancelledDomainEvent(
            Id,
            PatientId,
            DoctorId,
            SlotId,
            ScheduledAtUtc,
            cancelledAtUtc,
            isEarlyCancellation));

        if (isEarlyCancellation)
        {
            RaiseDomainEvent(new AppointmentRefundRequestedDomainEvent(
                Id,
                PatientId,
                DoctorId,
                SlotId));
        }
        else
        {
            RaiseDomainEvent(new AppointmentLateCancellationPolicyAppliedDomainEvent(
                Id,
                PatientId,
                DoctorId,
                doctorLateCancellationRetentionPercent));
        }

        return new AppointmentCancellationOutcome(
            isEarlyCancellation,
            isEarlyCancellation ? 0m : doctorLateCancellationRetentionPercent);
    }

    public void Reschedule(
        Guid newSlotId,
        DateTime newScheduledAtUtc,
        DateTime rescheduledAtUtc)
    {
        if (Status != AppointmentStatus.Confirmed)
        {
            throw new AppointmentNotReschedulableException(Status);
        }

        if (newSlotId == Guid.Empty)
        {
            throw new ArgumentException("New slot id is required.", nameof(newSlotId));
        }

        if (newScheduledAtUtc == default)
        {
            throw new ArgumentException("New scheduled time is required.", nameof(newScheduledAtUtc));
        }

        if (newScheduledAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("New scheduled time must be UTC.", nameof(newScheduledAtUtc));
        }

        if (rescheduledAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Reschedule time must be UTC.", nameof(rescheduledAtUtc));
        }

        if (newSlotId == SlotId && newScheduledAtUtc == ScheduledAtUtc)
        {
            return;
        }

        var previousSlotId = SlotId;
        var previousScheduledAtUtc = ScheduledAtUtc;

        SlotId = newSlotId;
        ScheduledAtUtc = newScheduledAtUtc;
        ReminderSentAtUtc = null;
        Touch();

        RaiseDomainEvent(new AppointmentRescheduledDomainEvent(
            Id,
            PatientId,
            DoctorId,
            previousSlotId,
            previousScheduledAtUtc,
            newSlotId,
            newScheduledAtUtc,
            rescheduledAtUtc));
    }
}
