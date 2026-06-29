using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Pharmacy;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.PharmacyOrders;

public sealed class PharmacyMedicationOrdersControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task ConfirmAsync_returns_confirmed_delivery_order_with_tracking()
    {
        var context = await MedicationOrderTestData.SeedPendingDeliveryOrderAsync(_host);
        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        var controller = new PharmacyMedicationOrdersController(_host.Sender);
        var result = await controller.ConfirmAsync(context.Order.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var order = Assert.IsType<MedicationOrderDto>(ok.Value);
        Assert.Equal("confirmed", order.Status);
        Assert.NotNull(order.TrackingUrl);
    }
}
