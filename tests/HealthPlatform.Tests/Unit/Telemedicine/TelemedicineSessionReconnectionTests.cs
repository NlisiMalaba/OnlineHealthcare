using HealthPlatform.Domain.Telemedicine;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class TelemedicineSessionReconnectionTests
{
    private static readonly TimeSpan GracePeriod = TimeSpan.FromSeconds(60);

    [Fact]
    public void BeginReconnectionGrace_sets_interrupted_timestamp_for_active_session()
    {
        var session = CreateActiveSession();
        var interruptedAt = DateTime.UtcNow;

        Assert.True(session.BeginReconnectionGrace(interruptedAt));
        Assert.Equal(interruptedAt, session.InterruptedAtUtc);
    }

    [Fact]
    public void TryCompleteReconnection_clears_interrupted_timestamp_within_grace()
    {
        var session = CreateActiveSession();
        var interruptedAt = DateTime.UtcNow;
        session.BeginReconnectionGrace(interruptedAt);

        Assert.True(session.TryCompleteReconnection(interruptedAt.AddSeconds(30), GracePeriod));
        Assert.Null(session.InterruptedAtUtc);
    }

    [Fact]
    public void TryCompleteReconnection_throws_after_grace_expires()
    {
        var session = CreateActiveSession();
        var interruptedAt = DateTime.UtcNow;
        session.BeginReconnectionGrace(interruptedAt);

        Assert.Throws<TelemedicineReconnectionGraceExpiredException>(() =>
            session.TryCompleteReconnection(interruptedAt.AddSeconds(61), GracePeriod));
    }

    [Fact]
    public void ExpireReconnectionGraceIfDue_marks_session_interrupted_after_grace()
    {
        var session = CreateActiveSession();
        var interruptedAt = DateTime.UtcNow;
        session.BeginReconnectionGrace(interruptedAt);

        Assert.True(session.ExpireReconnectionGraceIfDue(interruptedAt.AddSeconds(60), GracePeriod));
        Assert.Equal(TelemedicineSessionStatus.Interrupted, session.Status);
    }

    private static TelemedicineSession CreateActiveSession()
    {
        var session = TelemedicineSession.CreateForAppointment(Guid.CreateVersion7(), RtcProvider.Agora);
        session.MarkJoined(DateTime.UtcNow, null);
        return session;
    }
}
