namespace HealthPlatform.API.Requests.Appointments;

public sealed class BookAppointmentRequest
{
    public Guid DoctorId { get; init; }

    public Guid SlotId { get; init; }

    public DateTime ScheduledAtUtc { get; init; }
}
