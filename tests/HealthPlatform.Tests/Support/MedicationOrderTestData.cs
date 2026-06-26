using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.RegisterPharmacy;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Application.PharmacyOrders.CreateMedicationOrder;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Pharmacy;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Support;

public static class MedicationOrderTestData
{
    public sealed record SeededOrderContext(
        Patient Patient,
        Pharmacy Pharmacy,
        MedicationOrderDto Order);

    public static Task<SeededOrderContext> SeedPendingDeliveryOrderAsync(
        PatientRegistrationTestHost host,
        string medicationSku = "MED-001") =>
        SeedPendingOrderAsync(host, medicationSku, MedicationDeliveryType.Delivery, "12 Samora Machel Ave, Harare");

    public static Task<SeededOrderContext> SeedPendingPickupOrderAsync(
        PatientRegistrationTestHost host,
        string medicationSku = "MED-002") =>
        SeedPendingOrderAsync(host, medicationSku, MedicationDeliveryType.Pickup, null);

    private static async Task<SeededOrderContext> SeedPendingOrderAsync(
        PatientRegistrationTestHost host,
        string medicationSku,
        MedicationDeliveryType deliveryType,
        string? deliveryAddress)
    {
        var doctorRegistration = await host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await host.Sender.Send(
            new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId),
            CancellationToken.None);

        var doctor = await host.DbContext.Doctors.SingleAsync(d => d.Id == doctorRegistration.DoctorId);
        host.CurrentUser.UserId = doctor.UserId;

        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Workflow Patient",
                null,
                $"workflow-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();

        var prescription = await host.Sender.Send(
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

        var pharmacyRegistration = await host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var pharmacy = await host.DbContext.Pharmacies.SingleAsync(p => p.Id == pharmacyRegistration.PharmacyId);
        pharmacy.MarkVerified();
        await host.DbContext.SaveChangesAsync();

        host.PharmacyStockAvailability.SetInStock(pharmacy.Id, medicationSku);
        host.CurrentUser.UserId = patient.UserId;

        var order = await host.Sender.Send(
            new CreateMedicationOrderCommand(
                prescription.Id,
                pharmacy.Id,
                medicationSku,
                deliveryType,
                deliveryAddress),
            CancellationToken.None);

        return new SeededOrderContext(patient, pharmacy, order);
    }
}
