using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Wellness.HealthGoals.ListHealthGoals;

public sealed class ListHealthGoalsQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthGoalRepository healthGoalRepository)
    : IRequestHandler<ListHealthGoalsQuery, IReadOnlyList<HealthGoalDto>>
{
    public async Task<IReadOnlyList<HealthGoalDto>> Handle(ListHealthGoalsQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var goals = await healthGoalRepository.ListByPatientIdAsync(patient.Id, request.Status, ct);
        return goals.Select(goal => goal.ToDto()).ToList();
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
