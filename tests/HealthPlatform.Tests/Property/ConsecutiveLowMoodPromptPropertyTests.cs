using FsCheck.Xunit;
using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.MentalHealth;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class ConsecutiveLowMoodPromptPropertyTests
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc);

    // Feature: online-healthcare-platform, Property 34: Consecutive Low Mood Prompt
    [Property(Arbitrary = [typeof(MentalHealthArbitraries)], MaxTest = 100)]
    public bool Consecutive_low_mood_prompt_matches_trailing_low_rating_streak(
        ConsecutiveLowMoodPromptCase input) =>
        RunConsecutiveLowMoodPromptInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunConsecutiveLowMoodPromptInvariantAsync(ConsecutiveLowMoodPromptCase input)
    {
        var ratings = BuildRatings(input);
        var consecutiveLowRatings = MoodStreakPolicies.CountConsecutiveLowRatingsFromMostRecent(
            ratings.AsEnumerable().Reverse().ToList());

        var clock = new FakeTimeProvider(ReferenceNowUtc);
        await using var host = new PatientRegistrationTestHost(timeProvider: clock);

        var patient = await SeedPatientAsync(host);
        host.CurrentUser.UserId = patient.UserId;
        var controller = new MoodLogsController(host.Sender);

        for (var index = 0; index < ratings.Count; index++)
        {
            await controller.CreateAsync(
                new CreateMoodLogRequest
                {
                    Rating = ratings[index],
                    LoggedAtUtc = ReferenceNowUtc.AddHours(index - ratings.Count)
                },
                CancellationToken.None);
        }

        var hasNotifierCall = host.ConsecutiveLowMoodPromptNotifier.Calls.Count > 0;
        var hasOutboxEvent = await host.DbContext.DomainEventOutbox
            .AnyAsync(entry => entry.EventType.Contains("ConsecutiveLowMoodDetectedDomainEvent"));
        var hasPromptRecord = await host.DbContext.ConsecutiveLowMoodPrompts
            .AnyAsync(prompt => prompt.PatientId == patient.Id);

        var shouldPrompt = input.Expectation == ConsecutiveLowMoodPromptExpectation.ShouldPrompt;
        if (shouldPrompt && consecutiveLowRatings != MoodStreakPolicies.ConsecutiveLowMoodPromptThreshold)
        {
            return false;
        }

        if (!shouldPrompt && consecutiveLowRatings >= MoodStreakPolicies.ConsecutiveLowMoodPromptThreshold)
        {
            return false;
        }

        if (shouldPrompt)
        {
            return hasNotifierCall
                && hasOutboxEvent
                && hasPromptRecord
                && host.ConsecutiveLowMoodPromptNotifier.Calls.All(call =>
                    call.PatientId == patient.Id
                    && call.PatientUserId == patient.UserId);
        }

        return !hasNotifierCall && !hasOutboxEvent && !hasPromptRecord;
    }

    private static List<int> BuildRatings(ConsecutiveLowMoodPromptCase input)
    {
        var ratings = new List<int>(input.PrefixRatingCount + input.TrailingLowCount);

        for (var index = 0; index < input.PrefixRatingCount; index++)
        {
            ratings.Add((index % 4) + 2);
        }

        ratings.AddRange(Enumerable.Repeat(MoodStreakPolicies.LowMoodRating, input.TrailingLowCount));
        return ratings;
    }

    private static async Task<Patient> SeedPatientAsync(PatientRegistrationTestHost host)
    {
        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Consecutive Low Mood Property Patient",
                null,
                $"consecutive-low-mood-property-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }
}
