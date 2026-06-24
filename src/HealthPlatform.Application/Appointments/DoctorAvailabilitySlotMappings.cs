using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Appointments;

internal static class DoctorAvailabilitySlotMappings
{
    public static DoctorAvailabilitySlotDto ToDto(this DoctorAvailabilitySlot slot) =>
        new(
            slot.Id,
            slot.DayOfWeek,
            slot.StartTime,
            slot.EndTime,
            slot.SlotDurationMinutes,
            slot.AppointmentType,
            slot.IsActive);
}
