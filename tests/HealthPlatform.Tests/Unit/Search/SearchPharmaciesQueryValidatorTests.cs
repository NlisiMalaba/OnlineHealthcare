using HealthPlatform.Application.Search.SearchPharmacies;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class SearchPharmaciesQueryValidatorTests
{
    private readonly SearchPharmaciesQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenOnlyLatitudeProvided_FailsValidation()
    {
        var result = _validator.Validate(new SearchPharmaciesQuery(
            PatientLatitude: -17.8));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("PatientLatitude and PatientLongitude"));
    }

    [Fact]
    public void Validate_WithMedicationSkuAndGeoFilters_PassesValidation()
    {
        var result = _validator.Validate(new SearchPharmaciesQuery(
            MedicationSku: "MED-001",
            HasStock: true,
            PatientLatitude: -17.8,
            PatientLongitude: 31.0,
            Page: 1,
            PageSize: 20));

        Assert.True(result.IsValid);
    }
}
