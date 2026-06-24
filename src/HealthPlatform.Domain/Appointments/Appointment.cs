using HealthPlatform.Domain.Common;

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
}
