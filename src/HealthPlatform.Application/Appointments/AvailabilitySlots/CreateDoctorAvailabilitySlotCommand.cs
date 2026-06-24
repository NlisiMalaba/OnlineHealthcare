using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed record CreateDoctorAvailabilitySlotCommand(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    DoctorAppointmentType AppointmentType) : ICommand<DoctorAvailabilitySlotDto>;
