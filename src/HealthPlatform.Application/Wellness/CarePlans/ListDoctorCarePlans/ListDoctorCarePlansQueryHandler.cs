using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Wellness.CarePlans.ListDoctorCarePlans;

public sealed class ListDoctorCarePlansQueryHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    ICarePlanRepository carePlanRepository)
    : IRequestHandler<ListDoctorCarePlansQuery, IReadOnlyList<CarePlanDto>>
{
    public async Task<IReadOnlyList<CarePlanDto>> Handle(ListDoctorCarePlansQuery request, CancellationToken ct)
    {
        var doctor = await ResolveDoctorAsync(ct);
        var plans = await carePlanRepository.ListByDoctorIdAsync(doctor.Id, request.Status, ct);

        if (request.PatientId.HasValue)
        {
            plans = plans.Where(plan => plan.PatientId == request.PatientId.Value).ToList();
        }

        return plans.Select(plan => plan.ToDto()).ToList();
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
