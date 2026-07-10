using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Appointments;

namespace HealthPlatform.Application.Appointments.BookAppointment;

public sealed record BookAppointmentCommand(
    Guid DoctorId,
    Guid SlotId,
    DateTime ScheduledAtUtc,
    ConsultationType ConsultationType = ConsultationType.General) : ICommand<BookAppointmentDto>;
