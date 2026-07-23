using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using MediatR;

namespace HealthPlatform.Application.Wellness.CarePlans.AssignCarePlan;

public sealed class AssignCarePlanCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IPatientRepository patientRepository,
    ICarePlanRepository carePlanRepository,
    TimeProvider timeProvider)
    : IRequestHandler<AssignCarePlanCommand, CarePlanDto>
{
    public async Task<CarePlanDto> Handle(AssignCarePlanCommand request, CancellationToken ct)
    {
        var doctor = await ResolveVerifiedDoctorAsync(ct);
        var patient = await patientRepository.GetByIdAsync(request.PatientId, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        var assignedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var plan = CarePlan.Assign(
            patient.Id,
            doctor.Id,
            request.Condition,
            request.Tasks.Select(task => task.ToDraft()).ToList(),
            request.MonitoringTargets.Select(target => target.ToDraft()).ToList(),
            request.ReviewIntervalDays,
            assignedAtUtc);

        await carePlanRepository.AddAsync(plan, ct);
        return plan.ToDto();
    }

    private async Task<Doctor> ResolveVerifiedDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            throw new DomainException(
                WellnessErrorCodes.DoctorNotVerified,
                "Only verified doctors can assign care plans.");
        }

        return doctor;
    }
}
