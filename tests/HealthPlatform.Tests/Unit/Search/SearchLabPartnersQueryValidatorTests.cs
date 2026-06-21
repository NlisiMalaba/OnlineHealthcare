using HealthPlatform.Application.Search.SearchLabPartners;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class SearchLabPartnersQueryValidatorTests
{
    private readonly SearchLabPartnersQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenMinPriceGreaterThanMaxPrice_FailsValidation()
    {
        var result = _validator.Validate(new SearchLabPartnersQuery(
            MinPrice: 100m,
            MaxPrice: 50m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("MinPrice must be less than or equal to MaxPrice"));
    }

    [Fact]
    public void Validate_WithTestTypePriceAndGeoFilters_PassesValidation()
    {
        var result = _validator.Validate(new SearchLabPartnersQuery(
            TestType: "CBC",
            MinPrice: 10m,
            MaxPrice: 50m,
            PatientLatitude: -17.8,
            PatientLongitude: 31.0,
            Page: 1,
            PageSize: 20));

        Assert.True(result.IsValid);
    }
}
