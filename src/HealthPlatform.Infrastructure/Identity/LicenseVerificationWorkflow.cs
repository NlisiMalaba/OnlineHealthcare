using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Identity.Events;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Identity;

public sealed class LicenseVerificationWorkflow(
    IDoctorRepository doctorRepository,
    ILicenseVerificationQueueRepository licenseVerificationQueueRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    ILogger<LicenseVerificationWorkflow> logger) : ILicenseVerificationWorkflow
{
    public Task<LicenseVerificationResultDto> VerifyAsync(Guid doctorId, CancellationToken ct) =>
        TransitionAsync(
            doctorId,
            doctor => doctor.VerifyLicense(),
            ct);

    public Task<LicenseVerificationResultDto> RejectAsync(Guid doctorId, string reason, CancellationToken ct) =>
        TransitionAsync(
            doctorId,
            doctor => doctor.RejectLicense(reason),
            ct);

    private async Task<LicenseVerificationResultDto> TransitionAsync(
        Guid doctorId,
        Action<Doctor> transition,
        CancellationToken ct)
    {
        var doctor = await doctorRepository.GetByIdAsync(doctorId, ct)
            ?? throw new NotFoundException(
                IdentityErrorCodes.DoctorNotFound,
                "Doctor was not found.");

        if (doctor.VerificationStatus != DoctorVerificationStatus.Pending)
        {
            throw new DomainException(
                IdentityErrorCodes.InvalidVerificationState,
                "License verification can only be completed for doctors in pending state.");
        }

        try
        {
            transition(doctor);
        }
        catch (InvalidDoctorVerificationStatusException)
        {
            throw new DomainException(
                IdentityErrorCodes.InvalidVerificationState,
                "License verification can only be completed for doctors in pending state.");
        }

        await CompletePendingQueueItemAsync(doctorId, ct);
        await doctorRepository.UpdateAsync(doctor, ct);

        var domainEvent = doctor.DomainEvents.Last();
        await outboxRepository.EnqueueAsync(domainEvent, ct);
        await domainEventPublisher.PublishAsync(domainEvent, ct);
        doctor.ClearDomainEvents();

        logger.LogInformation(
            "Doctor {DoctorId} license verification transitioned to {VerificationStatus}.",
            doctor.Id,
            doctor.VerificationStatus);

        return MapResult(doctor);
    }

    private async Task CompletePendingQueueItemAsync(Guid doctorId, CancellationToken ct)
    {
        var queueItem = await licenseVerificationQueueRepository.GetPendingByDoctorIdAsync(doctorId, ct);
        if (queueItem is null)
        {
            return;
        }

        queueItem.MarkCompleted();
        await licenseVerificationQueueRepository.UpdateAsync(queueItem, ct);
    }

    private static LicenseVerificationResultDto MapResult(Doctor doctor) =>
        new(
            doctor.Id,
            doctor.VerificationStatus.ToString().ToLowerInvariant(),
            doctor.RejectionReason,
            doctor.UpdatedAtUtc);
}
