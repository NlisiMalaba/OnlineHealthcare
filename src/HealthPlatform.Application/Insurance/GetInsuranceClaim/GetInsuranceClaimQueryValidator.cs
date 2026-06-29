using FluentValidation;

namespace HealthPlatform.Application.Insurance.GetInsuranceClaim;

public sealed class GetInsuranceClaimQueryValidator : AbstractValidator<GetInsuranceClaimQuery>
{
    public GetInsuranceClaimQueryValidator()
    {
        RuleFor(x => x.ClaimId).NotEmpty();
    }
}
