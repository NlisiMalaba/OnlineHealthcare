using HealthPlatform.Domain.Telemedicine;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class TelemedicineSessionEndTests
{
    [Fact]
    public void End_marks_session_ended_and_raises_domain_event()
    {
        var session = CreateActiveSession();
        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var endedAt = DateTime.UtcNow;

        session.End(patientId, doctorId, endedAt);

        Assert.Equal(TelemedicineSessionStatus.Ended, session.Status);
        Assert.Equal(endedAt, session.EndedAtUtc);
        Assert.True(session.DurationSeconds >= 0);
        Assert.Single(session.DomainEvents);
    }

    [Fact]
    public void End_throws_when_session_has_not_started()
    {
        var session = TelemedicineSession.CreateForAppointment(Guid.CreateVersion7(), RtcProvider.Agora);

        Assert.Throws<TelemedicineSessionNotEndableException>(() =>
            session.End(Guid.CreateVersion7(), Guid.CreateVersion7(), DateTime.UtcNow));
    }

    private static TelemedicineSession CreateActiveSession()
    {
        var session = TelemedicineSession.CreateForAppointment(Guid.CreateVersion7(), RtcProvider.Agora);
        session.MarkJoined(DateTime.UtcNow.AddMinutes(-10), null);
        return session;
    }
}
