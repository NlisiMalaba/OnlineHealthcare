using FluentValidation;

namespace HealthPlatform.Application.HealthRecords.ListHealthRecordEntries;

public sealed class ListHealthRecordEntriesQueryValidator : AbstractValidator<ListHealthRecordEntriesQuery>
{
    public ListHealthRecordEntriesQueryValidator()
    {
        RuleFor(query => query.HealthRecordId)
            .NotEmpty();
    }
}
