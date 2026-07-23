using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Wellness.CarePlans.GetCarePlan;

public sealed class GetCarePlanQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    ICarePlanRepository carePlanRepository)
    : IRequestHandler<GetCarePlanQuery, CarePlanDto>
{
    public async Task<CarePlanDto> Handle(GetCarePlanQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var plan = await carePlanRepository.GetByIdForPatientAsync(request.CarePlanId, patient.Id, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.CarePlanNotFound,
                "Care plan was not found.");

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
