using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using MediatR;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed class UpdateDoctorAvailabilitySlotCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<UpdateDoctorAvailabilitySlotCommand, DoctorAvailabilitySlotDto>
{
    public async Task<DoctorAvailabilitySlotDto> Handle(UpdateDoctorAvailabilitySlotCommand request, CancellationToken ct)
    {
        var doctor = await ResolveDoctorAsync(ct);
        EnsureNoDuplicate(doctor, request);

        try
        {
            doctor.UpdateAvailabilitySlot(
                request.SlotId,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                request.SlotDurationMinutes,
                request.AppointmentType);
        }
        catch (KeyNotFoundException)
        {
            throw new NotFoundException(
                AppointmentErrorCodes.AvailabilitySlotNotFound,
                "Availability slot was not found.");
        }

        await doctorRepository.UpdateAsync(doctor, ct);
        await PublishPendingEventsAsync(doctor, ct);

        var slot = doctor.GetAvailabilitySlot(request.SlotId);
        return slot.ToDto();
    }

    private async Task<Domain.Identity.Doctor> ResolveDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");
    }

    private static void EnsureNoDuplicate(
        Domain.Identity.Doctor doctor,
        UpdateDoctorAvailabilitySlotCommand request)
    {
        var hasDuplicate = doctor.AvailabilitySlots.Any(slot =>
            slot.Id != request.SlotId
            && slot.DayOfWeek == request.DayOfWeek
            && slot.StartTime == request.StartTime
            && slot.EndTime == request.EndTime
            && slot.AppointmentType == request.AppointmentType);

        if (hasDuplicate)
        {
            throw new ConflictException(
                AppointmentErrorCodes.AvailabilitySlotConflict,
                "An identical availability slot already exists.");
        }
    }

    private async Task PublishPendingEventsAsync(Domain.Identity.Doctor doctor, CancellationToken ct)
    {
        var pendingEvents = doctor.DomainEvents.ToList();
        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        doctor.ClearDomainEvents();
    }
}
