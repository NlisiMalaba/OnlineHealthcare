using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Maternal.GrowthEntries;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Maternal;
using HealthPlatform.Domain.Maternal.Events;
using MediatR;

namespace HealthPlatform.Application.Maternal.GrowthEntries.RecordGrowthEntry;

public sealed class RecordGrowthEntryCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IChildProfileRepository childProfileRepository,
    IGrowthEntryRepository growthEntryRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<RecordGrowthEntryCommand, GrowthEntryDto>
{
    public async Task<GrowthEntryDto> Handle(RecordGrowthEntryCommand request, CancellationToken ct)
    {
        var guardian = await ResolveGuardianAsync(ct);
        var childProfile = await childProfileRepository.GetByIdAsync(request.ChildProfileId, ct)
            ?? throw new NotFoundException(
                GrowthEntryErrorCodes.ChildProfileNotFound,
                "Child profile was not found.");

        if (childProfile.GuardianId != guardian.Id)
        {
            throw new AccessDeniedException(
                GrowthEntryErrorCodes.AccessDenied,
                "You do not have access to this child profile.");
        }

        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var recordedAtUtc = request.RecordedAtUtc ?? createdAtUtc;
        if (recordedAtUtc.Kind != DateTimeKind.Utc)
        {
            recordedAtUtc = DateTime.SpecifyKind(recordedAtUtc, DateTimeKind.Utc);
        }

        var entry = GrowthEntry.Create(
            childProfile.Id,
            request.HeightCm,
            request.WeightKg,
            request.MilestoneNote,
            recordedAtUtc,
            createdAtUtc);

        await growthEntryRepository.AddAsync(entry, ct);
        await growthEntryRepository.SaveChangesAsync(ct);

        var assessment = ChildGrowthReferencePolicies.Assess(
            childProfile.DateOfBirth,
            entry.RecordedAtUtc,
            entry.HeightCm,
            entry.WeightKg);

        if (assessment.HasOutOfRangeMeasurement)
        {
            var domainEvent = new ChildGrowthOutOfRangeDetectedDomainEvent(
                entry.Id,
                childProfile.Id,
                guardian.Id,
                assessment.AgeMonths,
                assessment.HeightStatus,
                assessment.WeightStatus,
                entry.RecordedAtUtc);

            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        return GrowthEntryMappings.ToDto(entry, assessment);
    }

    private async Task<Domain.Identity.Patient> ResolveGuardianAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                GrowthEntryErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
