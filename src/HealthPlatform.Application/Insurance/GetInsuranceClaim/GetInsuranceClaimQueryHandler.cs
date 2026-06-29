using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Insurance.GetInsuranceClaim;

public sealed class GetInsuranceClaimQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IInsuranceClaimRepository claimRepository)
    : IRequestHandler<GetInsuranceClaimQuery, InsuranceClaimDto>
{
    public async Task<InsuranceClaimDto> Handle(GetInsuranceClaimQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var claim = await claimRepository.GetByIdForPatientAsync(request.ClaimId, patient.Id, ct)
            ?? throw new NotFoundException(
                InsuranceErrorCodes.ClaimNotFound,
                "Insurance claim was not found.");

        return claim.ToDto();
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");
    }
}
