using FsCheck.Xunit;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Application.Prescriptions.Dispensing;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class PrescriptionGuardPropertyTests
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc);

    // Feature: online-healthcare-platform, Property 11: Prescription Guard for Medication Orders
    [Property(Arbitrary = [typeof(PrescriptionArbitraries)], MaxTest = 100)]
    public bool Medication_order_dispensing_accepts_only_valid_prescriptions_once(
        PrescriptionDispensingCase input) =>
        RunPrescriptionGuardInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunPrescriptionGuardInvariantAsync(PrescriptionDispensingCase input)
    {
        var clock = new FakeTimeProvider(ReferenceNowUtc);
        await using var host = new PatientRegistrationTestHost(timeProvider: clock);

        var doctor = await SeedVerifiedDoctorAsync(host);
        var owner = await SeedPatientAsync(host, "owner");
        var prescription = await IssuePrescriptionAsync(
            host,
            clock,
            doctor,
            owner,
            input.DurationDays,
            ReferenceNowUtc.AddDays(-input.DaysSinceIssue));

        var dispensingPatient = input.Kind switch
        {
            PrescriptionDispensingScenarioKind.WrongPatient => await SeedPatientAsync(host, "other"),
            _ => owner
        };

        host.CurrentUser.UserId = dispensingPatient.UserId;

        return input.Kind switch
        {
            PrescriptionDispensingScenarioKind.Valid => await RunValidPrescriptionInvariantAsync(
                host,
                prescription.Id),
            PrescriptionDispensingScenarioKind.Expired => await RunRejectedDispenseInvariantAsync(
                host,
                prescription.Id,
                PrescriptionErrorCodes.PrescriptionExpired),
            PrescriptionDispensingScenarioKind.WrongPatient => await RunRejectedDispenseInvariantAsync(
                host,
                prescription.Id,
                PrescriptionErrorCodes.PrescriptionRequired),
            _ => false
        };
    }

    private static async Task<bool> RunValidPrescriptionInvariantAsync(
        PatientRegistrationTestHost host,
        Guid prescriptionId)
    {
        var firstAttempt = await host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(prescriptionId),
            CancellationToken.None);

        if (!string.Equals(firstAttempt.Status, "dispensed", StringComparison.Ordinal))
        {
            return false;
        }

        var stored = await host.DbContext.Prescriptions
            .AsNoTracking()
            .SingleAsync(p => p.Id == prescriptionId);

        if (stored.Status != PrescriptionStatus.Dispensed)
        {
            return false;
        }

        try
        {
            await host.Sender.Send(
                new DispensePrescriptionForMedicationOrderCommand(prescriptionId),
                CancellationToken.None);
            return false;
        }
        catch (DomainException ex)
        {
            return ex.Code == PrescriptionErrorCodes.PrescriptionDispensed;
        }
    }

    private static async Task<bool> RunRejectedDispenseInvariantAsync(
        PatientRegistrationTestHost host,
        Guid prescriptionId,
        string expectedCode)
    {
        try
        {
            await host.Sender.Send(
                new DispensePrescriptionForMedicationOrderCommand(prescriptionId),
                CancellationToken.None);
            return false;
        }
        catch (DomainException ex)
        {
            return ex.Code == expectedCode;
        }
    }

    private static async Task<Doctor> SeedVerifiedDoctorAsync(PatientRegistrationTestHost host)
    {
        var registration = await host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private static async Task<Patient> SeedPatientAsync(PatientRegistrationTestHost host, string suffix)
    {
        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Patient {suffix}",
                null,
                $"patient-{suffix}-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }

    private static async Task<PrescriptionDto> IssuePrescriptionAsync(
        PatientRegistrationTestHost host,
        FakeTimeProvider clock,
        Doctor doctor,
        Patient patient,
        int durationDays,
        DateTime issuedAtUtc)
    {
        host.CurrentUser.UserId = doctor.UserId;
        var restoreUtc = clock.GetUtcNow().UtcDateTime;
        clock.SetUtcNow(issuedAtUtc);

        try
        {
            return await host.Sender.Send(
                new CreatePrescriptionCommand(
                    patient.Id,
                    "Amoxicillin",
                    "500mg",
                    "Twice daily",
                    durationDays,
                    null,
                    null,
                    null),
                CancellationToken.None);
        }
        finally
        {
            clock.SetUtcNow(restoreUtc);
        }
    }
}
