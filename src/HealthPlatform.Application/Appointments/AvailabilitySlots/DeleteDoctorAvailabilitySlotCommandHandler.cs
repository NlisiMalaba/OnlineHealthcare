using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using MediatR;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed class DeleteDoctorAvailabilitySlotCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<DeleteDoctorAvailabilitySlotCommand>
{
    public async Task Handle(DeleteDoctorAvailabilitySlotCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        if (!doctor.RemoveAvailabilitySlot(request.SlotId))
        {
            throw new NotFoundException(
                AppointmentErrorCodes.AvailabilitySlotNotFound,
                "Availability slot was not found.");
        }

        await doctorRepository.UpdateAsync(doctor, ct);
        var pendingEvents = doctor.DomainEvents.ToList();
        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        doctor.ClearDomainEvents();
    }
}
