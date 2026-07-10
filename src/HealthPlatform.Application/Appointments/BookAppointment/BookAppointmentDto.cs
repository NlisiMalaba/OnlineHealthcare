using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Appointments.BookAppointment;

public sealed record BookAppointmentDto(
    Guid AppointmentId,
    Guid DoctorId,
    Guid SlotId,
    DateTime ScheduledAtUtc,
    string Status,
    DateTime SlotHoldExpiresAtUtc,
    ConsultationType ConsultationType,
    DoctorAppointmentType AppointmentType,
    AppointmentClinicDto? Clinic);
