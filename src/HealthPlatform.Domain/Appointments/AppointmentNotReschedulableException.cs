namespace HealthPlatform.Domain.Appointments;

public sealed class AppointmentNotReschedulableException(AppointmentStatus status)
    : Exception($"Appointment in status '{status}' cannot be rescheduled.")
{
    public AppointmentStatus Status { get; } = status;
}
