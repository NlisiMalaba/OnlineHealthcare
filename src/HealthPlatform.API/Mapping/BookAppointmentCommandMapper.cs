using HealthPlatform.API.Requests.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;

namespace HealthPlatform.API.Mapping;

public static class BookAppointmentCommandMapper
{
    public static BookAppointmentCommand ToCommand(BookAppointmentRequest request) =>
        new(request.DoctorId, request.SlotId, request.ScheduledAtUtc, request.ConsultationType);
}
