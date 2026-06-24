using HealthPlatform.API.Requests.Appointments;
using HealthPlatform.Application.Appointments.RescheduleAppointment;

namespace HealthPlatform.API.Mapping;

public static class RescheduleAppointmentCommandMapper
{
    public static RescheduleAppointmentCommand ToCommand(
        Guid appointmentId,
        RescheduleAppointmentRequest request) =>
        new(appointmentId, request.NewSlotId, request.NewScheduledAtUtc);
}
