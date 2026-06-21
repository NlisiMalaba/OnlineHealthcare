using HealthPlatform.API.Controllers;
using HealthPlatform.Application.Search.SearchPharmacies;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.Search;

public sealed class PharmacySearchControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task SearchAsync_WhenNoPharmaciesIndexed_ReturnsEmptyStatePayload()
    {
        var controller = new PharmacySearchController(_host.Sender);

        var result = await controller.SearchAsync(
            new SearchPharmaciesQuery(MedicationSku: "MED-001"),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<SearchPharmaciesResponseDto>(ok.Value);

        Assert.Empty(payload.Results);
        Assert.Equal("No pharmacies match your current search filters.", payload.EmptyStateMessage);
        Assert.NotNull(payload.EmptyStateSuggestion);
    }

    [Fact]
    public async Task SearchAsync_WithOnlyLatitude_ThrowsValidationException()
    {
        var controller = new PharmacySearchController(_host.Sender);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            controller.SearchAsync(
                new SearchPharmaciesQuery(PatientLatitude: -17.8),
                CancellationToken.None));
    }
}
