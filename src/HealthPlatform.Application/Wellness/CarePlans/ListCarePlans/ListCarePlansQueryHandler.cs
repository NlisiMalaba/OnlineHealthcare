using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Wellness.CarePlans.ListCarePlans;

public sealed class ListCarePlansQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    ICarePlanRepository carePlanRepository)
    : IRequestHandler<ListCarePlansQuery, IReadOnlyList<CarePlanDto>>
{
    public async Task<IReadOnlyList<CarePlanDto>> Handle(ListCarePlansQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var plans = await carePlanRepository.ListByPatientIdAsync(patient.Id, request.Status, ct);
        return plans.Select(plan => plan.ToDto()).ToList();
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
