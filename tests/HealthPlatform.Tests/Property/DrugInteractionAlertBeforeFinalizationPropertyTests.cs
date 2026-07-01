using FsCheck.Xunit;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class DrugInteractionAlertBeforeFinalizationPropertyTests
{
    private const string InteractingScheduleMedication = "Warfarin";
    private const string InteractingProposedMedication = "Ibuprofen";
    private const string NonInteractingScheduleMedication = "Amoxicillin";

    // Feature: online-healthcare-platform, Property 21: Drug Interaction Alert Before Finalization
    [Property(Arbitrary = [typeof(PrescriptionArbitraries)], MaxTest = 100)]
    public bool Drug_interaction_alert_precedes_prescription_finalization(DrugInteractionFinalizationCase input) =>
        RunDrugInteractionFinalizationInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunDrugInteractionFinalizationInvariantAsync(
        DrugInteractionFinalizationCase input)
    {
        var alertNotifier = new CapturingDrugInteractionAlertNotifier();
        await using var host = new PatientRegistrationTestHost(drugInteractionAlertNotifier: alertNotifier);

        var doctor = await SeedVerifiedDoctorAsync(host);
        var patient = await SeedPatientAsync(host);

        if (input.Scenario != DrugInteractionFinalizationScenario.EmptySchedule)
        {
            var scheduleMedication = input.Scenario == DrugInteractionFinalizationScenario.InteractingSchedule
                ? InteractingScheduleMedication
                : NonInteractingScheduleMedication;

            await SeedActiveScheduleAsync(host, patient.Id, scheduleMedication);
        }

        host.CurrentUser.UserId = doctor.UserId;
        var result = await host.Sender.Send(
            new CreatePrescriptionCommand(
                patient.Id,
                InteractingProposedMedication,
                "400mg",
                "Every 8 hours",
                input.DurationDays,
                null,
                null,
                null),
            CancellationToken.None);

        if (!string.Equals(result.Status, "active", StringComparison.Ordinal))
        {
            return false;
        }

        var outboxEntries = await host.DbContext.DomainEventOutbox
            .AsNoTracking()
            .OrderBy(entry => entry.OccurredAtUtc)
            .ToListAsync();

        var alertEntry = outboxEntries
            .LastOrDefault(entry => entry.EventType.Contains("DrugInteractionAlertDetectedDomainEvent"));
        var issuedEntry = outboxEntries
            .LastOrDefault(entry => entry.EventType.Contains("PrescriptionIssuedDomainEvent"));

        if (issuedEntry is null)
        {
            return false;
        }

        return input.Scenario switch
        {
            DrugInteractionFinalizationScenario.InteractingSchedule =>
                alertEntry is not null
                && alertNotifier.Calls.Count > 0
                && alertEntry.OccurredAtUtc <= issuedEntry.OccurredAtUtc
                && alertNotifier.Calls[0].InteractingMedicationName == InteractingScheduleMedication,
            DrugInteractionFinalizationScenario.NonInteractingSchedule or DrugInteractionFinalizationScenario.EmptySchedule =>
                alertEntry is null && alertNotifier.Calls.Count == 0,
            _ => false
        };
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
                "Property Interaction Patient",
                null,
                $"property-interaction-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }

    private static async Task SeedActiveScheduleAsync(
        PatientRegistrationTestHost host,
        Guid patientId,
        string medicationName)
    {
        var scheduleRepository = host.GetRequiredService<IMedicationScheduleRepository>();
        await scheduleRepository.AddAsync(
            MedicationSchedule.CreateActive(
                Guid.CreateVersion7(),
                patientId,
                medicationName,
                [new DateTime(2026, 6, 24, 8, 0, 0, DateTimeKind.Utc)]),
            CancellationToken.None);
    }
}
