using FluentValidation;
using HealthPlatform.Domain.ValueObjects;

namespace HealthPlatform.Application.Search.SearchPharmacies;

public sealed class SearchPharmaciesQueryValidator : AbstractValidator<SearchPharmaciesQuery>
{
    public SearchPharmaciesQueryValidator()
    {
        RuleFor(x => x.MedicationSku)
            .MaximumLength(64);

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
