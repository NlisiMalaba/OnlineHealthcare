using HealthPlatform.Domain.Telemedicine;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class TelemedicineSessionModeSwitchingTests
{
    [Fact]
    public void MarkJoined_applies_requested_mode_on_first_join()
    {
        var session = TelemedicineSession.CreateForAppointment(Guid.CreateVersion7(), RtcProvider.Agora);

        session.MarkJoined(DateTime.UtcNow, TelemedicineSessionMode.Audio);

        Assert.Equal(TelemedicineSessionMode.Audio, session.Mode);
        Assert.Equal(TelemedicineSessionStatus.Active, session.Status);
    }

    [Fact]
    public void MarkJoined_preserves_mode_when_no_mode_requested()
    {
        var session = TelemedicineSession.CreateForAppointment(Guid.CreateVersion7(), RtcProvider.Agora);
        session.MarkJoined(DateTime.UtcNow, TelemedicineSessionMode.Audio);

        session.MarkJoined(DateTime.UtcNow, null);

        Assert.Equal(TelemedicineSessionMode.Audio, session.Mode);
    }

    [Theory]
    [InlineData(TelemedicineSessionMode.Video)]
    [InlineData(TelemedicineSessionMode.Audio)]
    [InlineData(TelemedicineSessionMode.Chat)]
    public void MarkJoined_allows_switching_mode_while_session_is_active(TelemedicineSessionMode nextMode)
    {
        var session = TelemedicineSession.CreateForAppointment(Guid.CreateVersion7(), RtcProvider.Agora);
        session.MarkJoined(DateTime.UtcNow, TelemedicineSessionMode.Video);

        session.MarkJoined(DateTime.UtcNow, nextMode);

        Assert.Equal(nextMode, session.Mode);
        Assert.Equal(TelemedicineSessionStatus.Active, session.Status);
    }
}
