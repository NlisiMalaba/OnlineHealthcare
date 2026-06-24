using FluentValidation;
using HealthPlatform.Domain.ValueObjects;

namespace HealthPlatform.Application.Search.SearchDoctors;

public sealed class SearchDoctorsQueryValidator : AbstractValidator<SearchDoctorsQuery>
{
    public SearchDoctorsQueryValidator()
    {
        RuleFor(x => x.Specialty)
            .MaximumLength(120);

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 5)
            .When(x => x.MinRating.HasValue);

        RuleFor(x => x.MinFee)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinFee.HasValue);

        RuleFor(x => x.MaxFee)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxFee.HasValue);

        RuleFor(x => x)
            .Must(query => !query.MinFee.HasValue
                || !query.MaxFee.HasValue
                || query.MinFee <= query.MaxFee)
            .WithMessage("MinFee must be less than or equal to MaxFee.");

        RuleFor(x => x.PatientLatitude)
            .InclusiveBetween(GeoPoint.MinLatitude, GeoPoint.MaxLatitude)
            .When(x => x.PatientLatitude.HasValue);

        RuleFor(x => x.PatientLongitude)
            .InclusiveBetween(GeoPoint.MinLongitude, GeoPoint.MaxLongitude)
            .When(x => x.PatientLongitude.HasValue);

        RuleFor(x => x)
            .Must(query => query.PatientLatitude.HasValue == query.PatientLongitude.HasValue)
            .WithMessage("PatientLatitude and PatientLongitude must both be provided for proximity sorting.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, DoctorSearchOptions.MaxPageSize);
    }
}
