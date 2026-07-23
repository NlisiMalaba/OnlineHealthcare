using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Wellness.CarePlans.GetDoctorCarePlan;

public sealed class GetDoctorCarePlanQueryHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    ICarePlanRepository carePlanRepository)
    : IRequestHandler<GetDoctorCarePlanQuery, CarePlanDto>
{
    public async Task<CarePlanDto> Handle(GetDoctorCarePlanQuery request, CancellationToken ct)
    {
        var doctor = await ResolveDoctorAsync(ct);
        var plan = await carePlanRepository.GetByIdForDoctorAsync(request.CarePlanId, doctor.Id, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.CarePlanNotFound,
                "Care plan was not found.");

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
