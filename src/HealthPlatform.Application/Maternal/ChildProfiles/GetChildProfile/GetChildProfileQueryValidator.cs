using FluentValidation;

namespace HealthPlatform.Application.Maternal.ChildProfiles.GetChildProfile;

public sealed class GetChildProfileQueryValidator : AbstractValidator<GetChildProfileQuery>
{
    public GetChildProfileQueryValidator()
    {
        RuleFor(query => query.ChildProfileId)
            .NotEmpty();
    }
}
