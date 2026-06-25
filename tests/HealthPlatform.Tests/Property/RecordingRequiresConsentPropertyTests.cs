using FsCheck.Xunit;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.JoinSession;
using HealthPlatform.Application.Telemedicine.RecordingConsent;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class RecordingRequiresConsentPropertyTests
{
    // Feature: online-healthcare-platform, Property 10: Recording Requires Consent
    [Property(Arbitrary = [typeof(RecordingConsentArbitraries)], MaxTest = 100)]
    public bool Recording_enabled_implies_recording_consent(RecordingConsentOperationSequence sequence) =>
        RunRecordingConsentInvariantAsync(sequence).GetAwaiter().GetResult();

    private static async Task<bool> RunRecordingConsentInvariantAsync(RecordingConsentOperationSequence sequence)
    {
        await using var host = new PatientRegistrationTestHost();
        var context = await TelemedicineSessionTestContextFactory.CreateAsync(host);

        var consentGranted = false;
        var sessionStarted = false;

        foreach (var step in sequence.Steps)
        {
            switch (step)
            {
                case RecordingConsentStep.GrantConsent:
                    host.CurrentUser.UserId = context.PatientUserId;
                    try
                    {
                        await host.Sender.Send(
                            new GrantRecordingConsentCommand(context.AppointmentId),
                            CancellationToken.None);
                        consentGranted = true;
                    }
                    catch (DomainException ex) when (ex.Code == TelemedicineErrorCodes.RecordingConsentNotAllowed)
                    {
                        if (!sessionStarted)
                        {
                            return false;
                        }
                    }

                    break;

                case RecordingConsentStep.JoinSession:
                    host.CurrentUser.UserId = context.PatientUserId;
                    await host.Sender.Send(
                        new JoinTelemedicineSessionCommand(context.AppointmentId, null),
                        CancellationToken.None);
                    sessionStarted = true;
                    break;

                case RecordingConsentStep.EnableRecording:
                    host.CurrentUser.UserId = context.DoctorUserId;
                    try
                    {
                        await host.Sender.Send(
                            new EnableSessionRecordingCommand(context.AppointmentId),
                            CancellationToken.None);

                        if (!consentGranted)
                        {
                            return false;
                        }
                    }
                    catch (DomainException ex) when (ex.Code == TelemedicineErrorCodes.RecordingConsentRequired)
                    {
                        // Expected when consent was never granted.
                    }

                    break;
            }

            var session = await host.DbContext.TelemedicineSessions
                .AsNoTracking()
                .SingleAsync(s => s.AppointmentId == context.AppointmentId);

            if (session.RecordingEnabled && !session.RecordingConsent)
            {
                return false;
            }
        }

        var finalSession = await host.DbContext.TelemedicineSessions
            .AsNoTracking()
            .SingleAsync(s => s.AppointmentId == context.AppointmentId);

        return !finalSession.RecordingEnabled || finalSession.RecordingConsent;
    }
}
