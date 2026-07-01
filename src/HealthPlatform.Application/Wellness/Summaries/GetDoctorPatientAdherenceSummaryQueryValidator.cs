using FluentValidation;

namespace HealthPlatform.Application.Wellness.Summaries;

public sealed class GetDoctorPatientAdherenceSummaryQueryValidator
    : AbstractValidator<GetDoctorPatientAdherenceSummaryQuery>
{
    public GetDoctorPatientAdherenceSummaryQueryValidator()
    {
        RuleFor(query => query.PatientId).NotEmpty();
        RuleFor(query => query.Period).IsInEnum();
    }
}
