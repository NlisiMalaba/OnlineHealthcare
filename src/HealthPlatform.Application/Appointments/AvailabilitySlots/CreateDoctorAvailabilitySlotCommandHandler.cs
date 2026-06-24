using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using MediatR;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed class CreateDoctorAvailabilitySlotCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<CreateDoctorAvailabilitySlotCommand, DoctorAvailabilitySlotDto>
{
    public async Task<DoctorAvailabilitySlotDto> Handle(CreateDoctorAvailabilitySlotCommand request, CancellationToken ct)
    {
        var doctor = await ResolveDoctorAsync(ct);
        EnsureNoDuplicate(doctor, request.DayOfWeek, request.StartTime, request.EndTime, request.AppointmentType);

        var slot = doctor.AddAvailabilitySlot(
            request.DayOfWeek,
            request.StartTime,
            request.EndTime,
            request.SlotDurationMinutes,
            request.AppointmentType);

        await doctorRepository.AddAvailabilitySlotAsync(slot, ct);
        await PublishPendingEventsAsync(doctor, ct);
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
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        Domain.Identity.DoctorAppointmentType appointmentType)
    {
        var hasDuplicate = doctor.AvailabilitySlots.Any(slot =>
            slot.DayOfWeek == dayOfWeek
            && slot.StartTime == startTime
            && slot.EndTime == endTime
            && slot.AppointmentType == appointmentType);

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
