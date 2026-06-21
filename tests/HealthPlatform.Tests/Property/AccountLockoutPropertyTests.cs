using FsCheck;
using FsCheck.Xunit;
using HealthPlatform.Application.Auth;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Security;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Xunit;

namespace HealthPlatform.Tests.Properties;

public sealed class AccountLockoutPropertyTests
{
    // Feature: online-healthcare-platform, Property 31: Account Lockout After Failed Logins
    [Property(Arbitrary = [typeof(AuthArbitraries)], MaxTest = 500)]
    public Property Fewer_than_five_failures_do_not_lock_account_or_emit_event(int failureCount)
    {
        failureCount = Math.Abs(failureCount % 4) + 1;

        return Prop.ForAll(
            Arb.From(Gen.Elements(ApplicationRoles.Patient, ApplicationRoles.Doctor)),
            role => RunPreLockoutInvariantAsync(failureCount, role).GetAwaiter().GetResult());
    }

    // Feature: online-healthcare-platform, Property 31: Account Lockout After Failed Logins
    [Property(Arbitrary = [typeof(AuthArbitraries)], MaxTest = 500)]
    public Property Fifth_consecutive_failure_locks_account_and_emits_single_lockout_event(
        int failureCount,
        int repeatWhileLocked)
    {
        failureCount = Math.Abs(failureCount % 6) + 5;
        repeatWhileLocked = Math.Abs(repeatWhileLocked % 5);

        return Prop.ForAll(
            Arb.From(Gen.Elements(ApplicationRoles.Patient, ApplicationRoles.Doctor)),
            role => RunLockoutInvariantAsync(failureCount, repeatWhileLocked, role).GetAwaiter().GetResult());
    }

    private static async Task<bool> RunPreLockoutInvariantAsync(int failureCount, string role)
    {
        await using var host = new AuthTestHost();
        var email = $"pre-lockout-{Guid.NewGuid():N}@test.local";
        await host.CreateUserAsync(email, AuthTestHost.ValidPassword, [role]);

        for (var attempt = 0; attempt < failureCount; attempt++)
        {
            var ex = await Assert.ThrowsAsync<AppHttpException>(() =>
                host.LoginAsync(
                    new LoginCommand(email, AuthTestHost.WrongPassword, AuthTestHost.DeviceFingerprint)));

            if (ex.StatusCode != 401 || ex.ErrorCode != "INVALID_CREDENTIALS")
            {
                return false;
            }
        }

        if (await host.IsUserLockedOutAsync(email))
        {
            return false;
        }

        return await host.CountAccountLockedOutboxEventsAsync() == 0;
    }

    private static async Task<bool> RunLockoutInvariantAsync(int failureCount, int repeatWhileLocked, string role)
    {
        await using var host = new AuthTestHost();
        var email = $"lockout-{Guid.NewGuid():N}@test.local";
        await host.CreateUserAsync(email, AuthTestHost.ValidPassword, [role]);

        var lockedResponses = 0;
        for (var attempt = 0; attempt < failureCount; attempt++)
        {
            try
            {
                await host.LoginAsync(
                    new LoginCommand(email, AuthTestHost.WrongPassword, AuthTestHost.DeviceFingerprint));
                return false;
            }
            catch (AppHttpException ex)
            {
                if (ex.StatusCode == 423 && ex.ErrorCode == "ACCOUNT_LOCKED")
                {
                    lockedResponses++;
                    continue;
                }

                if (ex.StatusCode != 401 || ex.ErrorCode != "INVALID_CREDENTIALS")
                {
                    return false;
                }
            }
        }

        if (!await host.IsUserLockedOutAsync(email))
        {
            return false;
        }

        if (lockedResponses < 1)
        {
            return false;
        }

        if (await host.CountAccountLockedOutboxEventsAsync() != 1)
        {
            return false;
        }

        for (var attempt = 0; attempt < repeatWhileLocked; attempt++)
        {
            var ex = await Assert.ThrowsAsync<AppHttpException>(() =>
                host.LoginAsync(
                    new LoginCommand(email, AuthTestHost.WrongPassword, AuthTestHost.DeviceFingerprint)));

            if (ex.StatusCode != 423 || ex.ErrorCode != "ACCOUNT_LOCKED")
            {
                return false;
            }
        }

        return await host.CountAccountLockedOutboxEventsAsync() == 1;
    }
}
