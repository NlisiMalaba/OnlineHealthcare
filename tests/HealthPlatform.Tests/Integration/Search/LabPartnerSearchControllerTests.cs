using HealthPlatform.API.Controllers;
using HealthPlatform.Application.Search.SearchLabPartners;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.Search;

public sealed class LabPartnerSearchControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task SearchAsync_WhenNoLabPartnersIndexed_ReturnsEmptyStatePayload()
    {
        var controller = new LabPartnerSearchController(_host.Sender);

        var result = await controller.SearchAsync(
            new SearchLabPartnersQuery(TestType: "CBC"),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<SearchLabPartnersResponseDto>(ok.Value);

        Assert.Empty(payload.Results);
        Assert.Equal("No lab partners match your current search filters.", payload.EmptyStateMessage);
        Assert.NotNull(payload.EmptyStateSuggestion);
    }

    [Fact]
    public async Task SearchAsync_WithInvalidPriceRange_ThrowsValidationException()
    {
        var controller = new LabPartnerSearchController(_host.Sender);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            controller.SearchAsync(
                new SearchLabPartnersQuery(MinPrice: 100m, MaxPrice: 50m),
                CancellationToken.None));
    }
}
