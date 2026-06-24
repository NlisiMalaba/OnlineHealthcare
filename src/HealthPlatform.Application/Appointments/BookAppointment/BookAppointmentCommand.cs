using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Appointments.BookAppointment;

public sealed record BookAppointmentCommand(
    Guid DoctorId,
    Guid SlotId,
    DateTime ScheduledAtUtc) : ICommand<BookAppointmentDto>;
