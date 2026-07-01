using FluentValidation;

namespace HealthPlatform.Application.NextOfKin.GetNextOfKinContact;

public sealed class GetNextOfKinContactQueryValidator : AbstractValidator<GetNextOfKinContactQuery>
{
    public GetNextOfKinContactQueryValidator()
    {
        RuleFor(query => query.ContactId)
            .NotEmpty();
    }
}
