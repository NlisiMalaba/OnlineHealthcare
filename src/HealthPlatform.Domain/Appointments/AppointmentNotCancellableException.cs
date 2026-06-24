namespace HealthPlatform.Domain.Appointments;

public sealed class AppointmentNotCancellableException(AppointmentStatus status)
    : Exception($"Appointment in status '{status}' cannot be cancelled.")
{
    public AppointmentStatus Status { get; } = status;
}
