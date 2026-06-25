using HealthPlatform.Domain.Telemedicine;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class TelemedicineSessionRecordingTests
{
    [Fact]
    public void EnableRecording_without_consent_throws()
    {
        var session = TelemedicineSession.CreateForAppointment(Guid.CreateVersion7(), RtcProvider.Agora);

        Assert.Throws<RecordingConsentRequiredException>(() => session.EnableRecording());
        Assert.False(session.RecordingEnabled);
    }

    [Fact]
    public void EnableRecording_with_consent_sets_recording_enabled()
    {
        var session = TelemedicineSession.CreateForAppointment(Guid.CreateVersion7(), RtcProvider.Agora);
        session.GrantRecordingConsent(DateTime.UtcNow);

        session.EnableRecording();

        Assert.True(session.RecordingConsent);
        Assert.True(session.RecordingEnabled);
    }

    [Fact]
    public void GrantRecordingConsent_after_session_started_throws()
    {
        var session = TelemedicineSession.CreateForAppointment(Guid.CreateVersion7(), RtcProvider.Agora);
        session.MarkJoined(DateTime.UtcNow, null);

        Assert.Throws<RecordingConsentNotAllowedException>(() => session.GrantRecordingConsent(DateTime.UtcNow));
    }
}
