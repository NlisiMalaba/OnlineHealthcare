using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using MediatR;

namespace HealthPlatform.Application.Wellness.HealthGoals.UpdateHealthGoal;

public sealed class UpdateHealthGoalCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthGoalRepository healthGoalRepository)
    : IRequestHandler<UpdateHealthGoalCommand, HealthGoalDto>
{
    public async Task<HealthGoalDto> Handle(UpdateHealthGoalCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var goal = await healthGoalRepository.GetByIdForPatientAsync(request.GoalId, patient.Id, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.HealthGoalNotFound,
                "Health goal was not found.");

        if (goal.MetricType == WellnessMetricType.Custom && string.IsNullOrWhiteSpace(request.CustomLabel))
        {
            throw new DomainException(
                WellnessErrorCodes.HealthGoalMetricMismatch,
                "Custom label is required for custom metric goals.");
        }

        goal.Update(request.TargetValue, request.Unit, request.CustomLabel);
        await healthGoalRepository.UpdateAsync(goal, ct);
        return goal.ToDto();
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
}
