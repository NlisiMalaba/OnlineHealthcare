using HealthPlatform.API.Controllers;
using HealthPlatform.Application.PharmacyOrders.Dashboard;
using HealthPlatform.Application.PharmacyOrders.Inventory.AddInventoryItem;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.PharmacyOrders;

public sealed class PharmacyDashboardControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task GetDashboardAsync_returns_incoming_orders_status_counts_inventory_and_today_summary()
    {
        var context = await MedicationOrderTestData.SeedPendingDeliveryOrderAsync(_host);
        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        await _host.Sender.Send(
            new AddInventoryItemCommand("Paracetamol", "MED-001", 25, 10),
            CancellationToken.None);

        var controller = new PharmacyDashboardController(_host.Sender);
        var result = await controller.GetDashboardAsync(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dashboard = Assert.IsType<PharmacyDashboardDto>(ok.Value);

        Assert.Single(dashboard.IncomingOrders);
        var incoming = dashboard.IncomingOrders[0];
        Assert.Equal(context.Order.Id, incoming.OrderId);
        Assert.Equal(context.Order.PrescriptionId, incoming.PrescriptionId);
        Assert.Equal("12 Samora Machel Ave, Harare", incoming.DeliveryAddress);
        Assert.Equal("pending", incoming.PaymentStatus);
        Assert.Equal("pending", incoming.Status);

        Assert.Contains(dashboard.OrderStatusCounts, count => count.Status == "pending" && count.Count == 1);
        Assert.Single(dashboard.InventoryLevels);
        Assert.Equal("MED-001", dashboard.InventoryLevels[0].MedicationSku);
        Assert.Equal(0, dashboard.TodaySummary.PendingDeliveryCount);
        Assert.Equal(0, dashboard.TodaySummary.FulfilledOrderCount);
    }

    [Fact]
    public async Task GetDailySummaryAsync_counts_fulfilled_orders_and_pending_fulfillment()
    {
        var context = await MedicationOrderTestData.SeedPendingDeliveryOrderAsync(_host);
        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        var ordersController = new PharmacyMedicationOrdersController(_host.Sender);
        await ordersController.ConfirmAsync(context.Order.Id, CancellationToken.None);
        await ordersController.MarkDispatchedAsync(context.Order.Id, CancellationToken.None);
        await ordersController.CompleteFulfillmentAsync(context.Order.Id, CancellationToken.None);

        var controller = new PharmacyDashboardController(_host.Sender);
        var result = await controller.GetDailySummaryAsync(null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var summary = Assert.IsType<PharmacyDailySummaryDto>(ok.Value);

        Assert.Equal(1, summary.FulfilledOrderCount);
        Assert.Equal(0, summary.PendingDeliveryCount);
        Assert.Equal(0, summary.PendingPickupCount);
        Assert.Equal(0m, summary.RevenueAmount);
        Assert.Equal("USD", summary.Currency);
    }
}
