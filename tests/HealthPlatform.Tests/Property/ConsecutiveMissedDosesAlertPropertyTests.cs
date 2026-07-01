using FsCheck.Xunit;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class ConsecutiveMissedDosesAlertPropertyTests
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 6, 24, 18, 0, 0, DateTimeKind.Utc);

    // Feature: online-healthcare-platform, Property 20: Consecutive Missed Doses Alert
    [Property(Arbitrary = [typeof(WellnessArbitraries)], MaxTest = 100)]
    public bool Consecutive_missed_doses_notify_all_next_of_kin(ConsecutiveMissedDosesAlertCase input) =>
        RunConsecutiveMissedDosesAlertInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunConsecutiveMissedDosesAlertInvariantAsync(ConsecutiveMissedDosesAlertCase input)
    {
        var clock = new FakeTimeProvider(ReferenceNowUtc);
        await using var host = new PatientRegistrationTestHost(timeProvider: clock);

        var patient = await SeedPatientWithNextOfKinAsync(host, input.NextOfKinContactCount);
        var doseTimes = BuildOverdueDoseTimes(input.ConsecutiveMissedDoseCount);
        await SeedScheduleAsync(host, patient.Id, doseTimes);

        var dispatcher = host.GetRequiredService<IMissedDoseDetectionDispatcher>();
        var recorded = await dispatcher.RecordMissedDosesAsync(CancellationToken.None);

        if (recorded != input.ConsecutiveMissedDoseCount)
        {
            return false;
        }

        if (host.ConsecutiveMissedDosesNextOfKinNotifier.Calls.Count != 1)
        {
            return false;
        }

        var alertCall = host.ConsecutiveMissedDosesNextOfKinNotifier.Calls[0];
        if (alertCall.PatientId != patient.Id)
        {
            return false;
        }

        var expectedContactIds = await host.DbContext.NextOfKinContacts
            .AsNoTracking()
            .Where(contact => contact.PatientId == patient.Id)
            .Select(contact => contact.Id)
            .ToListAsync();

        if (expectedContactIds.Count != input.NextOfKinContactCount)
        {
            return false;
        }

        var notifiedContactIds = alertCall.ContactIds.ToHashSet();
        if (notifiedContactIds.Count != expectedContactIds.Count)
        {
            return false;
        }

        if (!expectedContactIds.All(contactId => notifiedContactIds.Contains(contactId)))
        {
            return false;
        }

        var hasAlertRecord = await host.DbContext.ConsecutiveMissedDoseAlerts
            .AnyAsync(alert => alert.PatientId == patient.Id);
        if (!hasAlertRecord)
        {
            return false;
        }

        return await host.DbContext.DomainEventOutbox
            .AnyAsync(entry => entry.EventType.Contains("ConsecutiveMissedDosesDetectedDomainEvent"));
    }

    private static DateTime[] BuildOverdueDoseTimes(int consecutiveMissedDoseCount) =>
        Enumerable.Range(1, consecutiveMissedDoseCount)
            .Select(index => ReferenceNowUtc
                .Subtract(WellnessPolicies.MissedDoseGracePeriod)
                .Subtract(TimeSpan.FromHours(consecutiveMissedDoseCount - index + 1)))
            .ToArray();

    private static async Task<Patient> SeedPatientWithNextOfKinAsync(
        PatientRegistrationTestHost host,
        int contactCount)
    {
        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Consecutive Missed Property Patient",
                null,
                $"consecutive-missed-property-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        var repository = host.GetRequiredService<INextOfKinRepository>();

        for (var index = 0; index < contactCount; index++)
        {
            await repository.AddAsync(
                NextOfKinContact.Create(
                    patient.Id,
                    $"Contact {index + 1}",
                    "Sibling",
                    $"+1555000{index:0000}",
                    $"contact-{index}-{Guid.NewGuid():N}@example.com",
                    index % 2 == 0),
                CancellationToken.None);
        }

        return patient;
    }

    private static async Task SeedScheduleAsync(
        PatientRegistrationTestHost host,
        Guid patientId,
        IReadOnlyList<DateTime> doseTimes)
    {
        var schedule = MedicationSchedule.CreateActive(
            Guid.CreateVersion7(),
            patientId,
            "Amoxicillin",
            doseTimes);

        await host.GetRequiredService<IMedicationScheduleRepository>().AddAsync(schedule, CancellationToken.None);
    }
}
