using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Appointments.CancelAppointment;

public sealed record CancelAppointmentCommand(
    Guid AppointmentId,
    string? Reason) : ICommand<CancelAppointmentDto>;
