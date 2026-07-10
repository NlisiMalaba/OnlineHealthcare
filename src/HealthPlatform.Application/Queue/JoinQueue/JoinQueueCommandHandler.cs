using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Queue;
using MediatR;

namespace HealthPlatform.Application.Queue.JoinQueue;

public sealed class JoinQueueCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    IQueueEntryRepository queueEntryRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<JoinQueueCommand, QueueEntryDto>
{
    public async Task<QueueEntryDto> Handle(JoinQueueCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var patient = await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                QueueErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        var appointment = await appointmentRepository.GetByIdForPatientAsync(request.AppointmentId, patient.Id, ct)
            ?? throw new NotFoundException(
                QueueErrorCodes.AppointmentNotFound,
                "Appointment was not found.");

        if (appointment.Status != AppointmentStatus.Confirmed)
        {
            throw new DomainException(
                QueueErrorCodes.AppointmentNotConfirmed,
                "Only confirmed appointments can join the virtual queue.");
        }

        if (await queueEntryRepository.ExistsActiveForAppointmentAsync(appointment.Id, ct))
        {
            throw new ConflictException(
                QueueErrorCodes.QueueEntryAlreadyExists,
                "An active queue entry already exists for this appointment.");
        }

        var doctor = await doctorRepository.GetByIdWithSlotsAsync(appointment.DoctorId, ct)
            ?? throw new NotFoundException(
                QueueErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        var slot = doctor.AvailabilitySlots.SingleOrDefault(s => s.Id == appointment.SlotId && s.IsActive);
        if (slot is null)
        {
            throw new NotFoundException(
                QueueErrorCodes.AvailabilitySlotNotFound,
                "Availability slot was not found.");
        }

        if (!PhysicalAppointmentRules.SupportsVirtualQueue(slot.AppointmentType))
        {
            throw new DomainException(
                QueueErrorCodes.AppointmentNotPhysical,
                "Only physical clinic appointments can join the virtual queue.");
        }

        var activeEntryCount = await queueEntryRepository.CountActiveByDoctorIdAsync(doctor.Id, ct);
        var queuePosition = QueueWaitTimeCalculator.ComputeQueuePosition(activeEntryCount);
        var averageConsultationDurationMinutes = QueueConsultationDurationResolver.ResolveAverageMinutes(
            doctor.AvailabilitySlots);
        var estimatedWaitMinutes = QueueWaitTimeCalculator.ComputeEstimatedWaitMinutes(
            activeEntryCount,
            averageConsultationDurationMinutes);

        var joinedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var entry = QueueEntry.Create(
            appointment.Id,
            patient.Id,
            doctor.Id,
            patient.FullName,
            appointment.ScheduledAtUtc,
            queuePosition,
            estimatedWaitMinutes,
            joinedAtUtc);

        await queueEntryRepository.AddAsync(entry, ct);
        foreach (var domainEvent in entry.DomainEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        entry.ClearDomainEvents();

        return entry.ToDto();
    }
}
