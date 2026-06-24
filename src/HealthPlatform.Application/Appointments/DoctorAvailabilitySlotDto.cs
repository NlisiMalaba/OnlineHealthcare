using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Appointments;

public sealed record DoctorAvailabilitySlotDto(
    Guid Id,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    DoctorAppointmentType AppointmentType,
    bool IsActive);
