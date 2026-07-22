using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Wellness.HealthGoals.GetHealthGoal;

public sealed class GetHealthGoalQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthGoalRepository healthGoalRepository)
    : IRequestHandler<GetHealthGoalQuery, HealthGoalDto>
{
    public async Task<HealthGoalDto> Handle(GetHealthGoalQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var goal = await healthGoalRepository.GetByIdForPatientAsync(request.GoalId, patient.Id, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.HealthGoalNotFound,
                "Health goal was not found.");

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
