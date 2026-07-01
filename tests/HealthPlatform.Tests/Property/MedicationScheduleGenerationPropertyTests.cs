using FsCheck.Xunit;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Application.Prescriptions.Dispensing;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class MedicationScheduleGenerationPropertyTests
{
    private static readonly DateTime ReferenceIssueUtc = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    // Feature: online-healthcare-platform, Property 18: Medication Schedule Generation
    [Property(Arbitrary = [typeof(PrescriptionArbitraries)], MaxTest = 100)]
    public bool Dispensed_prescription_generates_schedule_with_correct_dose_times(
        MedicationScheduleGenerationCase input) =>
        RunMedicationScheduleGenerationInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunMedicationScheduleGenerationInvariantAsync(
        MedicationScheduleGenerationCase input)
    {
        var dispensedAtUtc = ReferenceIssueUtc
            .AddDays(1)
            .AddHours(input.DispenseHourUtc)
            .AddMinutes(input.DispenseMinuteUtc);

        var clock = new FakeTimeProvider(dispensedAtUtc);
        await using var host = new PatientRegistrationTestHost(timeProvider: clock);

        var doctor = await SeedVerifiedDoctorAsync(host);
        var patient = await SeedPatientAsync(host);
        var prescription = await IssuePrescriptionAsync(
            host,
            clock,
            doctor,
            patient,
            input.Frequency,
            input.DurationDays,
            ReferenceIssueUtc);

        host.CurrentUser.UserId = patient.UserId;
        var dispenseResult = await host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(prescription.Id),
            CancellationToken.None);

        if (!string.Equals(dispenseResult.Status, "dispensed", StringComparison.Ordinal))
        {
            return false;
        }

        var schedule = await host.DbContext.MedicationSchedules
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.PrescriptionId == prescription.Id);

        if (schedule is null)
        {
            return false;
        }

        var expectedDoseTimes = MedicationDoseSchedulePolicies.BuildDoseTimes(
            input.Frequency,
            input.DurationDays,
            dispensedAtUtc);

        return schedule.PatientId == patient.Id
            && schedule.MedicationName == "Amoxicillin"
            && schedule.Status == MedicationScheduleStatus.Active
            && schedule.DoseTimes.Count > 0
            && schedule.DoseTimes.SequenceEqual(expectedDoseTimes)
            && schedule.DoseTimes.All(doseTime => doseTime >= dispensedAtUtc)
            && IsStrictlyAscending(schedule.DoseTimes);
    }

    private static bool IsStrictlyAscending(IReadOnlyList<DateTime> doseTimes)
    {
        for (var index = 1; index < doseTimes.Count; index++)
        {
            if (doseTimes[index] <= doseTimes[index - 1])
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<Doctor> SeedVerifiedDoctorAsync(PatientRegistrationTestHost host)
    {
        var registration = await host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private static async Task<Patient> SeedPatientAsync(PatientRegistrationTestHost host)
    {
        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Patient schedule",
                null,
                $"patient-schedule-{Guid.NewGuid():N}@example.com",
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
        string frequency,
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
                    frequency,
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
