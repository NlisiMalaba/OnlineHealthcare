namespace HealthPlatform.Application.Appointments.BookAppointment;

public sealed record BookAppointmentDto(
    Guid AppointmentId,
    Guid DoctorId,
    Guid SlotId,
    DateTime ScheduledAtUtc,
    string Status,
    DateTime SlotHoldExpiresAtUtc);
