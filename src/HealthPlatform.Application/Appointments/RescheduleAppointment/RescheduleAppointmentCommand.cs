using MediatR;

namespace HealthPlatform.Application.Appointments.RescheduleAppointment;

public sealed record RescheduleAppointmentCommand(
    Guid AppointmentId,
    Guid NewSlotId,
    DateTime NewScheduledAtUtc) : IRequest<RescheduleAppointmentDto>;
