using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Appointments.RescheduleAppointment;

public sealed record RescheduleAppointmentDto(
    Guid AppointmentId,
    Guid SlotId,
    DateTime ScheduledAtUtc,
    DateTime? PreviousScheduledAtUtc,
    string Status,
    DoctorAppointmentType AppointmentType,
    AppointmentClinicDto? Clinic);
