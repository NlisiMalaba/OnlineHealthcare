using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Insurance.GetInsuranceClaim;

public sealed record GetInsuranceClaimQuery(Guid ClaimId) : IQuery<InsuranceClaimDto>;
