using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Wellness.HealthGoals;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using MediatR;

namespace HealthPlatform.Application.Wellness.WellnessEntries.RecordWellnessEntry;

public sealed class RecordWellnessEntryCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthGoalRepository healthGoalRepository,
    IWellnessEntryRepository wellnessEntryRepository,
    TimeProvider timeProvider)
    : IRequestHandler<RecordWellnessEntryCommand, WellnessEntryDto>
{
    public async Task<WellnessEntryDto> Handle(RecordWellnessEntryCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var recordedAtUtc = NormalizeUtc(request.RecordedAtUtc ?? createdAtUtc);

        Guid? linkedGoalId = null;
        if (request.GoalId.HasValue)
        {
            var linkedGoal = await healthGoalRepository.GetByIdForPatientAsync(request.GoalId.Value, patient.Id, ct)
                ?? throw new NotFoundException(
                    WellnessErrorCodes.HealthGoalNotFound,
                    "Health goal was not found.");

            if (linkedGoal.Status != HealthGoalStatus.Active)
            {
                throw new DomainException(
                    WellnessErrorCodes.HealthGoalNotActive,
                    "Only active health goals can be linked to a wellness entry.");
            }

            if (linkedGoal.MetricType != request.MetricType)
            {
                throw new DomainException(
                    WellnessErrorCodes.HealthGoalMetricMismatch,
                    "Wellness entry metric type must match the linked health goal.");
            }

            linkedGoalId = linkedGoal.Id;
        }

        var entry = WellnessEntry.Create(
            patient.Id,
            linkedGoalId,
            request.MetricType,
            request.Value,
            recordedAtUtc,
            createdAtUtc);

        await wellnessEntryRepository.AddAsync(entry, ct);
        await wellnessEntryRepository.SaveChangesAsync(ct);

        var activeGoals = await healthGoalRepository.ListActiveByPatientAndMetricAsync(
            patient.Id,
            request.MetricType,
            ct);

        var progress = HealthGoalProgressCalculator.CalculateForActiveGoals(
            activeGoals,
            request.MetricType,
            request.Value);

        return entry.ToDto(progress);
    }

    private async Task<Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
