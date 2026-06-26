using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Pharmacy;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.RegisterPharmacy;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.PharmacyOrders;

public sealed class MedicationOrdersControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task CreateAsync_ReturnsCreatedOrderForPatient()
    {
        var (patientUserId, prescriptionId, pharmacyId) = await SeedAsync();
        _host.PharmacyStockAvailability.SetInStock(pharmacyId, "MED-001");
        _host.CurrentUser.UserId = patientUserId;

        var controller = new MedicationOrdersController(_host.Sender);
        var result = await controller.CreateAsync(
            new CreateMedicationOrderRequest
            {
                PrescriptionId = prescriptionId,
                PharmacyId = pharmacyId,
                MedicationSku = "MED-001",
                DeliveryType = MedicationDeliveryType.Delivery,
                DeliveryAddress = "45 Jason Moyo Ave, Harare"
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var order = Assert.IsType<MedicationOrderDto>(created.Value);
        Assert.Equal("pending", order.Status);
        Assert.Equal("delivery", order.DeliveryType);
    }

    private async Task<(Guid PatientUserId, Guid PrescriptionId, Guid PharmacyId)> SeedAsync()
    {
        var doctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(
            new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == doctorRegistration.DoctorId);
        _host.CurrentUser.UserId = doctor.UserId;

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Controller Order Patient",
                null,
                $"controller-order-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();

        var prescription = await _host.Sender.Send(
            new CreatePrescriptionCommand(
                patient.Id,
                "Paracetamol",
                "500mg",
                "Every 8 hours",
                5,
                null,
                null,
                null),
            CancellationToken.None);

        var pharmacyRegistration = await _host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var pharmacy = await _host.DbContext.Pharmacies.SingleAsync(p => p.Id == pharmacyRegistration.PharmacyId);
        pharmacy.MarkVerified();
        await _host.DbContext.SaveChangesAsync();

        return (patient.UserId, prescription.Id, pharmacy.Id);
    }
}
