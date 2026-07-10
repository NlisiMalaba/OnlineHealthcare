namespace HealthPlatform.API.Requests.Appointments;

using HealthPlatform.Domain.Appointments;

public sealed class BookAppointmentRequest
{
    public Guid DoctorId { get; init; }

    public Guid SlotId { get; init; }

    public DateTime ScheduledAtUtc { get; init; }

    public ConsultationType ConsultationType { get; init; } = ConsultationType.General;
}
