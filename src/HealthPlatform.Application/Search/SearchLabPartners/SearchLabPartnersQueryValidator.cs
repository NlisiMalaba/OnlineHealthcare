using FluentValidation;
using HealthPlatform.Domain.ValueObjects;

namespace HealthPlatform.Application.Search.SearchLabPartners;

public sealed class SearchLabPartnersQueryValidator : AbstractValidator<SearchLabPartnersQuery>
{
    public SearchLabPartnersQueryValidator()
    {
        RuleFor(x => x.TestType)
            .MaximumLength(120);

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(query => !query.MinPrice.HasValue
                || !query.MaxPrice.HasValue
                || query.MinPrice <= query.MaxPrice)
            .WithMessage("MinPrice must be less than or equal to MaxPrice.");

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
