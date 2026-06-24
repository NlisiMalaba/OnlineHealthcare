namespace HealthPlatform.API.Requests.Appointments;

public sealed class RescheduleAppointmentRequest
{
    public Guid NewSlotId { get; init; }

    public DateTime NewScheduledAtUtc { get; init; }
}
