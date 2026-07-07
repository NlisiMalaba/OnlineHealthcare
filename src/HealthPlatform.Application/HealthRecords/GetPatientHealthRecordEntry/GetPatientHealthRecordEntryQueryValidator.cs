using FluentValidation;

namespace HealthPlatform.Application.HealthRecords.GetPatientHealthRecordEntry;

public sealed class GetPatientHealthRecordEntryQueryValidator : AbstractValidator<GetPatientHealthRecordEntryQuery>
{
    public GetPatientHealthRecordEntryQueryValidator()
    {
        RuleFor(query => query.EntryId)
            .NotEmpty()
            .MaximumLength(32);
    }
}
