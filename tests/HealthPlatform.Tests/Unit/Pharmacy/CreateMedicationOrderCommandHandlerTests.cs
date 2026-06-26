using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.RegisterPharmacy;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Application.PharmacyOrders.CreateMedicationOrder;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.PharmacyOrders;

public sealed class CreateMedicationOrderCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Handle_WithStockAndValidPrescription_CreatesOrderDispensesPrescriptionAndNotifiesPharmacy()
    {
        var (patient, prescription, pharmacy) = await SeedOrderPrerequisitesAsync();
        _host.PharmacyStockAvailability.SetInStock(pharmacy.Id, "MED-001");
        _host.CurrentUser.UserId = patient.UserId;

        var order = await _host.Sender.Send(
            new CreateMedicationOrderCommand(
                prescription.Id,
                pharmacy.Id,
                "MED-001",
                MedicationDeliveryType.Delivery,
                "12 Samora Machel Ave, Harare"),
            CancellationToken.None);

        Assert.Equal("pending", order.Status);
        Assert.Equal("delivery", order.DeliveryType);
        Assert.Equal("12 Samora Machel Ave, Harare", order.DeliveryAddress);

        var updatedPrescription = await _host.DbContext.Prescriptions.SingleAsync(p => p.Id == prescription.Id);
        Assert.Equal(PrescriptionStatus.Dispensed, updatedPrescription.Status);

        Assert.Single(_host.PharmacyOrderRealtimeNotifier.Published);
        Assert.Equal(pharmacy.Id, _host.PharmacyOrderRealtimeNotifier.Published[0].PharmacyId);
        Assert.Equal(order.Id, _host.PharmacyOrderRealtimeNotifier.Published[0].Order.OrderId);

        Assert.Single(_host.PharmacyOrderReceivedNotifier.Notifications);
        Assert.Equal(pharmacy.UserId, _host.PharmacyOrderReceivedNotifier.Notifications[0].PharmacyUserId);
        Assert.Equal(order.Id, _host.PharmacyOrderReceivedNotifier.Notifications[0].OrderId);
    }

    [Fact]
    public async Task Handle_WhenPharmacyHasNoStock_ThrowsDomainException()
    {
        var (patient, prescription, pharmacy) = await SeedOrderPrerequisitesAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _host.Sender.Send(
                new CreateMedicationOrderCommand(
                    prescription.Id,
                    pharmacy.Id,
                    "MED-001",
                    MedicationDeliveryType.Pickup,
                    null),
                CancellationToken.None));

        Assert.Equal(PharmacyErrorCodes.MedicationOutOfStock, exception.Code);
    }

    [Fact]
    public async Task Handle_WhenPrescriptionAlreadyDispensed_ThrowsDomainException()
    {
        var (patient, prescription, pharmacy) = await SeedOrderPrerequisitesAsync();
        _host.PharmacyStockAvailability.SetInStock(pharmacy.Id, "MED-001");
        _host.CurrentUser.UserId = patient.UserId;

        await _host.Sender.Send(
            new CreateMedicationOrderCommand(
                prescription.Id,
                pharmacy.Id,
                "MED-001",
                MedicationDeliveryType.Pickup,
                null),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _host.Sender.Send(
                new CreateMedicationOrderCommand(
                    prescription.Id,
                    pharmacy.Id,
                    "MED-001",
                    MedicationDeliveryType.Pickup,
                    null),
                CancellationToken.None));

        Assert.Equal("PRESCRIPTION_DISPENSED", exception.Code);
    }

    private async Task<(Patient patient, Prescription prescription, Domain.Identity.Pharmacy pharmacy)> SeedOrderPrerequisitesAsync()
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
                "Order Patient",
                null,
                $"order-patient-{Guid.NewGuid():N}@example.com",
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

        var prescriptionEntity = await _host.DbContext.Prescriptions.SingleAsync(p => p.Id == prescription.Id);
        return (patient, prescriptionEntity, pharmacy);
    }
}
