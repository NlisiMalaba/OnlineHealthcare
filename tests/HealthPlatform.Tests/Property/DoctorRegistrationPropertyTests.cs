using FsCheck.Xunit;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class DoctorRegistrationPropertyTests
{
    // Feature: online-healthcare-platform, Property 4: Doctor Registration Starts in Pending State
    [Property(Arbitrary = [typeof(DoctorRegistrationArbitraries)], MaxTest = 100)]
    public bool Doctor_registration_starts_in_pending_state(ValidDoctorRegistration registration) =>
        RunPendingStateInvariantAsync(registration).GetAwaiter().GetResult();

    private static async Task<bool> RunPendingStateInvariantAsync(ValidDoctorRegistration registration)
    {
        await using var host = new PatientRegistrationTestHost();

        var response = await host.Sender.Send(registration.ToCommand(), CancellationToken.None);

        if (!string.Equals(response.VerificationStatus, "pending", StringComparison.Ordinal))
        {
            return false;
        }

        var doctor = await host.DbContext.Doctors
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == response.DoctorId);

        return doctor is not null
            && doctor.VerificationStatus == DoctorVerificationStatus.Pending;
    }
}
