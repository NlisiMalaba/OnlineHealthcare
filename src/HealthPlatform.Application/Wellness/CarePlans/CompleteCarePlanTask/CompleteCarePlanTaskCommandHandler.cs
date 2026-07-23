using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using MediatR;

namespace HealthPlatform.Application.Wellness.CarePlans.CompleteCarePlanTask;

public sealed class CompleteCarePlanTaskCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    ICarePlanRepository carePlanRepository,
    TimeProvider timeProvider)
    : IRequestHandler<CompleteCarePlanTaskCommand, CarePlanDto>
{
    public async Task<CarePlanDto> Handle(CompleteCarePlanTaskCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var plan = await carePlanRepository.GetByIdForPatientAsync(request.CarePlanId, patient.Id, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.CarePlanNotFound,
                "Care plan was not found.");

        if (plan.Status != CarePlanStatus.Active)
        {
            throw new DomainException(
                WellnessErrorCodes.CarePlanNotActive,
                "Care plan is not active.");
        }

        var completedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        try
        {
            plan.CompleteTask(request.TaskId, completedAtUtc);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already completed", StringComparison.Ordinal))
        {
            throw new ConflictException(
                WellnessErrorCodes.CarePlanTaskAlreadyCompleted,
                "Care plan task is already completed.");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("was not found", StringComparison.Ordinal))
        {
            throw new NotFoundException(
                WellnessErrorCodes.CarePlanTaskNotFound,
                "Care plan task was not found.");
        }

        await carePlanRepository.UpdateAsync(plan, ct);
        return plan.ToDto();
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
