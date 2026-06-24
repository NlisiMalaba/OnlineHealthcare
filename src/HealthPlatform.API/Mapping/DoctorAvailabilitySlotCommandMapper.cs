using HealthPlatform.API.Requests.Appointments;
using HealthPlatform.Application.Appointments.AvailabilitySlots;

namespace HealthPlatform.API.Mapping;

public static class DoctorAvailabilitySlotCommandMapper
{
    public static CreateDoctorAvailabilitySlotCommand ToCreateCommand(DoctorAvailabilitySlotUpsertRequest request) =>
        new(
            request.DayOfWeek,
            TimeOnly.Parse(request.StartTime),
            TimeOnly.Parse(request.EndTime),
            request.SlotDurationMinutes,
            request.AppointmentType);

    public static UpdateDoctorAvailabilitySlotCommand ToUpdateCommand(
        Guid slotId,
        DoctorAvailabilitySlotUpsertRequest request) =>
        new(
            slotId,
            request.DayOfWeek,
            TimeOnly.Parse(request.StartTime),
            TimeOnly.Parse(request.EndTime),
            request.SlotDurationMinutes,
            request.AppointmentType);
}
