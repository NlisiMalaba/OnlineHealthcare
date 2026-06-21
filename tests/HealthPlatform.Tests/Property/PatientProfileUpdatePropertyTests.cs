using FsCheck.Xunit;
using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class PatientProfileUpdatePropertyTests
{
    // Feature: online-healthcare-platform, Property 2: Profile Update Round Trip
    [Property(Arbitrary = [typeof(PatientProfileUpdateArbitraries)], MaxTest = 100)]
    public bool Profile_update_round_trip_returns_persisted_values(ProfileUpdateRoundTripCase roundTrip) =>
        RunProfileUpdateRoundTripInvariantAsync(roundTrip).GetAwaiter().GetResult();

    private static async Task<bool> RunProfileUpdateRoundTripInvariantAsync(ProfileUpdateRoundTripCase roundTrip)
    {
        await using var host = new PatientRegistrationTestHost();

        var registration = await host.Sender.Send(
            roundTrip.Registration.ToCommand(),
            CancellationToken.None);

        var patient = await host.DbContext.Patients
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == registration.PatientId);
        if (patient is null)
        {
            return false;
        }

        host.CurrentUser.UserId = patient.UserId;

        var update = roundTrip.ProfileUpdate;
        PatientProfileDto response;
        try
        {
            response = await host.Sender.Send(update.ToCommand(), CancellationToken.None);
        }
        catch
        {
            return false;
        }

        if (!ProfileMatchesUpdate(response, update, registration.PatientId))
        {
            return false;
        }

        host.DbContext.ChangeTracker.Clear();
        var reloaded = await host.DbContext.Patients
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == registration.PatientId);

        return reloaded is not null && PersistedProfileMatchesUpdate(reloaded, update);
    }

    private static bool ProfileMatchesUpdate(
        PatientProfileDto profile,
        ValidPatientProfileUpdate update,
        Guid patientId) =>
        profile.PatientId == patientId
        && profile.FullName == update.FullName.Trim()
        && profile.DateOfBirth == update.DateOfBirth
        && profile.BloodType == update.BloodType
        && ListsEqual(profile.KnownAllergies, update.KnownAllergies)
        && ListsEqual(profile.ChronicConditions, update.ChronicConditions);

    private static bool PersistedProfileMatchesUpdate(Patient patient, ValidPatientProfileUpdate update) =>
        patient.FullName == update.FullName.Trim()
        && patient.DateOfBirth == update.DateOfBirth
        && patient.BloodType == update.BloodType
        && ListsEqual(patient.KnownAllergies, update.KnownAllergies)
        && ListsEqual(patient.ChronicConditions, update.ChronicConditions);

    private static bool ListsEqual(IReadOnlyList<string> actual, IReadOnlyList<string> expected)
    {
        var normalizedActual = NormalizeList(actual);
        var normalizedExpected = NormalizeList(expected);
        return normalizedActual.SequenceEqual(normalizedExpected, StringComparer.OrdinalIgnoreCase);
    }

    private static List<string> NormalizeList(IReadOnlyList<string> values) =>
        values
            .Select(v => v.Trim())
            .Where(v => v.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}
