using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Appointments;
using MediatR;

namespace HealthPlatform.Application.Appointments.CancelAppointment;

public sealed class CancelAppointmentCommandHandler(
    TimeProvider timeProvider,
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    ISlotHoldService slotHoldService,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<CancelAppointmentCommand, CancelAppointmentDto>
{
    public async Task<CancelAppointmentDto> Handle(CancelAppointmentCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var patient = await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        var appointment = await appointmentRepository.GetByIdForPatientAsync(request.AppointmentId, patient.Id, ct)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.AppointmentNotFound,
                "Appointment was not found.");

        var doctor = await doctorRepository.GetByIdAsync(appointment.DoctorId, ct)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        AppointmentCancellationOutcome outcome;
        try
        {
            outcome = appointment.Cancel(
                timeProvider.GetUtcNow().UtcDateTime,
                AppointmentPolicies.EarlyCancellationWindow,
                doctor.LateCancellationRetentionPercent,
                request.Reason);
        }
        catch (AppointmentNotCancellableException)
        {
            throw new DomainException(
                AppointmentErrorCodes.AppointmentNotCancellable,
                "Only confirmed appointments can be cancelled.");
        }

        if (outcome.IsEarlyCancellation)
        {
            await slotHoldService.ReleaseHoldAsync(appointment.SlotId, ct);
        }

        await appointmentRepository.UpdateAsync(appointment, ct);

        var pendingEvents = appointment.DomainEvents.ToList();
        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        appointment.ClearDomainEvents();

        return new CancelAppointmentDto(
            appointment.Id,
            "cancelled",
            outcome.IsEarlyCancellation,
            outcome.IsEarlyCancellation,
            outcome.IsEarlyCancellation,
            outcome.AppliedLateCancellationRetentionPercent);
    }
}
