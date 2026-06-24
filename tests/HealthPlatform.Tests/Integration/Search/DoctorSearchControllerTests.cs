using HealthPlatform.API.Controllers;
using HealthPlatform.Application.Search.SearchDoctors;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.Search;

public sealed class DoctorSearchControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task SearchAsync_WhenNoDoctorsIndexed_ReturnsEmptyStatePayload()
    {
        var controller = new DoctorSearchController(_host.Sender);

        var result = await controller.SearchAsync(
            new SearchDoctorsQuery(Specialty: "Cardiology"),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<SearchDoctorsResponseDto>(ok.Value);

        Assert.Empty(payload.Results);
        Assert.Equal("No doctors match your current search filters.", payload.EmptyStateMessage);
        Assert.NotNull(payload.EmptyStateSuggestion);
    }

    [Fact]
    public async Task SearchAsync_WithInvalidFeeRange_ThrowsValidationException()
    {
        var controller = new DoctorSearchController(_host.Sender);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            controller.SearchAsync(
                new SearchDoctorsQuery(MinFee: 300m, MaxFee: 100m),
                CancellationToken.None));
    }
}
