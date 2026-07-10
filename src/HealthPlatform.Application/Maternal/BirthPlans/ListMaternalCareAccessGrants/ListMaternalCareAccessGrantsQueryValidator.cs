using FluentValidation;

namespace HealthPlatform.Application.Maternal.BirthPlans.ListMaternalCareAccessGrants;

public sealed class ListMaternalCareAccessGrantsQueryValidator
    : AbstractValidator<ListMaternalCareAccessGrantsQuery>
{
    public ListMaternalCareAccessGrantsQueryValidator()
    {
        RuleFor(query => query.AntenatalRecordId)
            .NotEmpty();
    }
}
