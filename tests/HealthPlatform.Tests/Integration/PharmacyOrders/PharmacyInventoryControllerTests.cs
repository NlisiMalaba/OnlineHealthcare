using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Pharmacy;
using HealthPlatform.Application.Identity.RegisterPharmacy;
using HealthPlatform.Application.PharmacyOrders.Inventory;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.PharmacyOrders;

public sealed class PharmacyInventoryControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task AddAsync_creates_inventory_item()
    {
        var pharmacy = await SeedVerifiedPharmacyAsync();
        _host.CurrentUser.UserId = pharmacy.UserId;

        var controller = new PharmacyInventoryController(_host.Sender);
        var result = await controller.AddAsync(
            new AddInventoryItemRequest
            {
                MedicationName = "Paracetamol",
                MedicationSku = "MED-100",
                Quantity = 30
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var item = Assert.IsType<InventoryItemDto>(created.Value);
        Assert.Equal("MED-100", item.MedicationSku);
        Assert.Equal(30, item.Quantity);
    }

    private async Task<Domain.Identity.Pharmacy> SeedVerifiedPharmacyAsync()
    {
        var registration = await _host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var pharmacy = await _host.DbContext.Pharmacies.SingleAsync(p => p.Id == registration.PharmacyId);
        pharmacy.MarkVerified();
        await _host.DbContext.SaveChangesAsync();
        return pharmacy;
    }
}
