using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Insurance.ListPatientInsuranceClaims;

public sealed class ListPatientInsuranceClaimsQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IInsuranceClaimRepository claimRepository)
    : IRequestHandler<ListPatientInsuranceClaimsQuery, IReadOnlyList<InsuranceClaimListItemDto>>
{
    public async Task<IReadOnlyList<InsuranceClaimListItemDto>> Handle(
        ListPatientInsuranceClaimsQuery request,
        CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var claims = await claimRepository.ListForPatientAsync(patient.Id, ct);
        return claims.Select(claim => claim.ToListItemDto()).ToList();
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");
    }
}
