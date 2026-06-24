using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Appointments;
using MediatR;

namespace HealthPlatform.Application.Appointments.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandHandler(
    TimeProvider timeProvider,
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    ISlotHoldService slotHoldService,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<RescheduleAppointmentCommand, RescheduleAppointmentDto>
{
    public async Task<RescheduleAppointmentDto> Handle(RescheduleAppointmentCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        var appointment = await appointmentRepository.GetByIdForDoctorAsync(request.AppointmentId, doctor.Id, ct)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.AppointmentNotFound,
                "Appointment was not found.");

        var previousScheduledAtUtc = appointment.ScheduledAtUtc;
        var previousSlotId = appointment.SlotId;

        if (request.NewSlotId != previousSlotId
            || request.NewScheduledAtUtc != previousScheduledAtUtc)
        {
            var slotExists = doctor.AvailabilitySlots.Any(
                slot => slot.Id == request.NewSlotId && slot.IsActive);

            if (!slotExists)
            {
                throw new NotFoundException(
                    AppointmentErrorCodes.AvailabilitySlotNotFound,
                    "Availability slot was not found.");
            }

            if (request.NewSlotId != previousSlotId
                && await slotHoldService.IsSlotHeldAsync(request.NewSlotId, ct))
            {
                throw new ConflictException(
                    AppointmentErrorCodes.SlotUnavailable,
                    "The selected slot is currently unavailable.");
            }

            var hasConflict = await appointmentRepository.ExistsConfirmedForSlotAtTimeAsync(
                request.NewSlotId,
                request.NewScheduledAtUtc,
                appointment.Id,
                ct);

            if (hasConflict)
            {
                throw new ConflictException(
                    AppointmentErrorCodes.SlotUnavailable,
                    "The selected slot is currently unavailable.");
            }
        }

        try
        {
            appointment.Reschedule(
                request.NewSlotId,
                request.NewScheduledAtUtc,
                timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (AppointmentNotReschedulableException)
        {
            throw new DomainException(
                AppointmentErrorCodes.AppointmentNotReschedulable,
                "Only confirmed appointments can be rescheduled.");
        }

        if (previousSlotId != appointment.SlotId)
        {
            await slotHoldService.ReleaseHoldAsync(previousSlotId, ct);
        }

        await appointmentRepository.UpdateAsync(appointment, ct);

        var pendingEvents = appointment.DomainEvents.ToList();
        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        appointment.ClearDomainEvents();

        var wasRescheduled = previousScheduledAtUtc != appointment.ScheduledAtUtc
            || previousSlotId != appointment.SlotId;

        return new RescheduleAppointmentDto(
            appointment.Id,
            appointment.SlotId,
            appointment.ScheduledAtUtc,
            wasRescheduled ? previousScheduledAtUtc : null,
            "confirmed");
    }
}
