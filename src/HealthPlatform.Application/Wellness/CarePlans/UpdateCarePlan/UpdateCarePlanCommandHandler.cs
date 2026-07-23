using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using MediatR;

namespace HealthPlatform.Application.Wellness.CarePlans.UpdateCarePlan;

public sealed class UpdateCarePlanCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    ICarePlanRepository carePlanRepository,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateCarePlanCommand, CarePlanDto>
{
    public async Task<CarePlanDto> Handle(UpdateCarePlanCommand request, CancellationToken ct)
    {
        var doctor = await ResolveDoctorAsync(ct);
        var plan = await carePlanRepository.GetByIdForDoctorAsync(request.CarePlanId, doctor.Id, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.CarePlanNotFound,
                "Care plan was not found.");

        if (plan.Status != CarePlanStatus.Active)
        {
            throw new DomainException(
                WellnessErrorCodes.CarePlanNotActive,
                "Care plan is not active.");
        }

        var updatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var existingById = plan.Tasks.ToDictionary(task => task.Id);
        var taskDrafts = request.Tasks.Select(task =>
        {
            var draft = task.ToDraft();
            if (draft.Id != Guid.Empty && existingById.TryGetValue(draft.Id, out var existing))
            {
                return draft with
                {
                    CompletedAtUtc = existing.CompletedAtUtc,
                    ReminderSentAtUtc = existing.ReminderSentAtUtc
                };
            }

            return draft;
        }).ToList();

        try
        {
            plan.Update(
                request.Condition,
                taskDrafts,
                request.MonitoringTargets.Select(target => target.ToDraft()).ToList(),
                request.ReviewIntervalDays,
                request.NextReviewAt,
                updatedAtUtc);
        }
        catch (InvalidOperationException)
        {
            throw new DomainException(
                WellnessErrorCodes.CarePlanNotActive,
                "Care plan is not active.");
        }

        await carePlanRepository.UpdateAsync(plan, ct);
        return plan.ToDto();
    }

    private async Task<Doctor> ResolveDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");
    }
}
