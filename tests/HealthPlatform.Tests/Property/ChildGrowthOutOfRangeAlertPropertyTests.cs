using FsCheck.Xunit;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;
using HealthPlatform.Application.Maternal.GrowthEntries.RecordGrowthEntry;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Maternal;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class ChildGrowthOutOfRangeAlertPropertyTests
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc);

    // Feature: online-healthcare-platform, Property 35: Child Growth Out-of-Range Alert
    [Property(Arbitrary = [typeof(ChildGrowthOutOfRangeAlertArbitraries)], MaxTest = 100)]
    public bool Child_growth_out_of_range_alert_matches_reference_assessment(
        ChildGrowthOutOfRangeAlertCase input) =>
        RunChildGrowthOutOfRangeAlertInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunChildGrowthOutOfRangeAlertInvariantAsync(
        ChildGrowthOutOfRangeAlertCase input)
    {
        var dateOfBirth = DateOnly.FromDateTime(ReferenceNowUtc.AddMonths(-input.AgeMonths));
        var (heightCm, weightKg) = BuildMeasurements(input.AgeMonths, input.Expectation);

        var assessment = ChildGrowthReferencePolicies.Assess(
            dateOfBirth,
            ReferenceNowUtc,
            heightCm,
            weightKg);

        var shouldAlert = assessment.HasOutOfRangeMeasurement;
        var expectationRequiresAlert = input.Expectation != ChildGrowthMeasurementExpectation.InRange;

        if (shouldAlert != expectationRequiresAlert)
        {
            return false;
        }

        var clock = new FakeTimeProvider(ReferenceNowUtc);
        await using var host = new PatientRegistrationTestHost(timeProvider: clock);

        var guardian = await SeedGuardianAsync(host);
        host.CurrentUser.UserId = guardian.UserId;

        var profile = await host.Sender.Send(
            new CreateChildProfileCommand(
                $"Property Child {input.AgeMonths}",
                dateOfBirth,
                null,
                []),
            CancellationToken.None);

        host.ChildGrowthOutOfRangeNotifier.Calls.Clear();

        await host.Sender.Send(
            new RecordGrowthEntryCommand(
                profile.Id,
                heightCm,
                weightKg,
                null,
                ReferenceNowUtc),
            CancellationToken.None);

        var hasNotifierCall = host.ChildGrowthOutOfRangeNotifier.Calls.Count > 0;
        var hasOutboxEvent = await host.DbContext.DomainEventOutbox
            .AnyAsync(entry => entry.EventType.Contains("ChildGrowthOutOfRangeDetectedDomainEvent"));

        if (shouldAlert)
        {
            return hasNotifierCall
                && hasOutboxEvent
                && host.ChildGrowthOutOfRangeNotifier.Calls.All(call =>
                    call.GuardianUserId == guardian.UserId
                    && call.ChildProfileId == profile.Id
                    && call.HeightStatus == assessment.HeightStatus
                    && call.WeightStatus == assessment.WeightStatus);
        }

        return !hasNotifierCall && !hasOutboxEvent;
    }

    private static (decimal? HeightCm, decimal? WeightKg) BuildMeasurements(
        int ageMonths,
        ChildGrowthMeasurementExpectation expectation)
    {
        var heightCurves = ChildGrowthReferencePolicies.BuildHeightReferenceCurves(ageMonths);
        var weightCurves = ChildGrowthReferencePolicies.BuildWeightReferenceCurves(ageMonths);
        var heightP3 = PercentileValueAtAge(heightCurves, 3, ageMonths);
        var heightP97 = PercentileValueAtAge(heightCurves, 97, ageMonths);
        var heightP50 = PercentileValueAtAge(heightCurves, 50, ageMonths);
        var weightP3 = PercentileValueAtAge(weightCurves, 3, ageMonths);
        var weightP97 = PercentileValueAtAge(weightCurves, 97, ageMonths);
        var weightP50 = PercentileValueAtAge(weightCurves, 50, ageMonths);

        return expectation switch
        {
            ChildGrowthMeasurementExpectation.OutOfRangeHeightBelow =>
                (Math.Max(1m, heightP3 - 5m), null),
            ChildGrowthMeasurementExpectation.OutOfRangeHeightAbove =>
                (heightP97 + 10m, null),
            ChildGrowthMeasurementExpectation.OutOfRangeWeightBelow =>
                (null, Math.Max(1m, weightP3 - 1m)),
            ChildGrowthMeasurementExpectation.OutOfRangeWeightAbove =>
                (null, weightP97 + 5m),
            ChildGrowthMeasurementExpectation.InRange =>
                (heightP50, weightP50),
            _ => throw new ArgumentOutOfRangeException(nameof(expectation), expectation, null)
        };
    }

    private static decimal PercentileValueAtAge(
        IReadOnlyList<ChildGrowthPercentileCurve> curves,
        int percentile,
        int ageMonths) =>
        curves.First(curve => curve.Percentile == percentile).Points[ageMonths].Value;

    private static async Task<Patient> SeedGuardianAsync(PatientRegistrationTestHost host)
    {
        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Child Growth Property Guardian",
                null,
                $"child-growth-property-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
