using FluentValidation;

namespace HealthPlatform.Application.HealthRecords.GetHealthRecordEntry;

public sealed class GetHealthRecordEntryQueryValidator : AbstractValidator<GetHealthRecordEntryQuery>
{
    public GetHealthRecordEntryQueryValidator()
    {
        RuleFor(query => query.EntryId)
            .NotEmpty()
            .MaximumLength(32);
    }
}
