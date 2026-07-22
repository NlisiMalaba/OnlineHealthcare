using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using MediatR;

namespace HealthPlatform.Application.Wellness.HealthGoals.CreateHealthGoal;

public sealed class CreateHealthGoalCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthGoalRepository healthGoalRepository,
    TimeProvider timeProvider)
    : IRequestHandler<CreateHealthGoalCommand, HealthGoalDto>
{
    public async Task<HealthGoalDto> Handle(CreateHealthGoalCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var unit = string.IsNullOrWhiteSpace(request.Unit)
            ? HealthGoalMappings.ResolveDefaultUnit(request.MetricType)
            : request.Unit;

        var goal = HealthGoal.Create(
            patient.Id,
            request.MetricType,
            request.TargetValue,
            unit,
            request.CustomLabel,
            timeProvider.GetUtcNow().UtcDateTime);

        await healthGoalRepository.AddAsync(goal, ct);
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
