using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Insurance.ListPatientInsuranceClaims;

public sealed record ListPatientInsuranceClaimsQuery : IQuery<IReadOnlyList<InsuranceClaimListItemDto>>;
