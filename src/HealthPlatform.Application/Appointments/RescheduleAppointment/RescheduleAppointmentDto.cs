namespace HealthPlatform.Application.Appointments.RescheduleAppointment;

public sealed record RescheduleAppointmentDto(
    Guid AppointmentId,
    Guid SlotId,
    DateTime ScheduledAtUtc,
    DateTime? PreviousScheduledAtUtc,
    string Status);
