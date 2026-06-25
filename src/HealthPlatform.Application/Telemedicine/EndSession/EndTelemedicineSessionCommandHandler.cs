using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Telemedicine.Realtime;
using HealthPlatform.Domain.Telemedicine;
using MediatR;

namespace HealthPlatform.Application.Telemedicine.EndSession;

public sealed class EndTelemedicineSessionCommandHandler(
    TimeProvider timeProvider,
    ITelemedicineSessionParticipantService participantService,
    IAppointmentRepository appointmentRepository,
    ITelemedicineSessionRepository telemedicineSessionRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<EndTelemedicineSessionCommand, EndTelemedicineSessionDto>
{
    public async Task<EndTelemedicineSessionDto> Handle(EndTelemedicineSessionCommand request, CancellationToken ct)
    {
        var participant = await participantService.ResolveParticipantAsync(
            request.AppointmentId,
            requireActiveSession: false,
            ct);

        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.AppointmentNotFound,
                "Appointment was not found.");

        var session = participant.Session;
        var endedAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            session.End(appointment.PatientId, appointment.DoctorId, endedAtUtc);
        }
        catch (TelemedicineSessionNotEndableException)
        {
            throw new DomainException(
                TelemedicineErrorCodes.SessionNotEndable,
                "Telemedicine session cannot be ended in its current state.");
        }

        await telemedicineSessionRepository.UpdateAsync(session, ct);

        var pendingEvents = session.DomainEvents.ToList();
        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        session.ClearDomainEvents();

        return new EndTelemedicineSessionDto(
            session.Id,
            session.AppointmentId,
            session.Status.ToString().ToLowerInvariant(),
            session.DurationSeconds,
            endedAtUtc,
            session.SessionSummaryRef);
    }
}
