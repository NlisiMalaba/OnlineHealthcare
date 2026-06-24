using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed record UpdateDoctorAvailabilitySlotCommand(
    Guid SlotId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    DoctorAppointmentType AppointmentType) : ICommand<DoctorAvailabilitySlotDto>;
