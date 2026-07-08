using HealthPlatform.Application.Labs.GetPatientLabResultDownload;
using Xunit;

namespace HealthPlatform.Tests.Unit.Labs;

public sealed class GetPatientLabResultDownloadQueryValidatorTests
{
    [Fact]
    public void Validator_rejects_empty_lab_result_id()
    {
        var validator = new GetPatientLabResultDownloadQueryValidator();
        var result = validator.Validate(new GetPatientLabResultDownloadQuery(Guid.Empty));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_accepts_valid_lab_result_id()
    {
        var validator = new GetPatientLabResultDownloadQueryValidator();
        var result = validator.Validate(new GetPatientLabResultDownloadQuery(Guid.CreateVersion7()));

        Assert.True(result.IsValid);
    }
}
