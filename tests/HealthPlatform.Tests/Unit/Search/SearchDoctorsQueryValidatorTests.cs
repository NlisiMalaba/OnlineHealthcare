using HealthPlatform.Application.Search.SearchDoctors;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class SearchDoctorsQueryValidatorTests
{
    private readonly SearchDoctorsQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenMinFeeGreaterThanMaxFee_FailsValidation()
    {
        var result = _validator.Validate(new SearchDoctorsQuery(
            MinFee: 200m,
            MaxFee: 100m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == string.Empty);
    }

    [Fact]
    public void Validate_WhenOnlyLatitudeProvided_FailsValidation()
    {
        var result = _validator.Validate(new SearchDoctorsQuery(
            PatientLatitude: -17.8));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("PatientLatitude and PatientLongitude"));
    }

    [Fact]
    public void Validate_WithValidCriteria_PassesValidation()
    {
        var result = _validator.Validate(new SearchDoctorsQuery(
            Specialty: "Cardiology",
            MinRating: 4,
            MinFee: 20m,
            MaxFee: 100m,
            HasAvailability: true,
            PatientLatitude: -17.8,
            PatientLongitude: 31.0,
            Page: 1,
            PageSize: 20));

        Assert.True(result.IsValid);
    }
}
