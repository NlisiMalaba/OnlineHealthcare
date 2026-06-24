using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Domain.Telemedicine;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class TelemedicineSessionSummaryBuilderTests
{
    [Fact]
    public void Build_includes_session_metadata_for_health_record_attachment()
    {
        var sessionId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var startedAt = new DateTime(2026, 6, 24, 10, 0, 0, DateTimeKind.Utc);
        var endedAt = startedAt.AddMinutes(25);

        var summary = new TelemedicineSessionSummaryRecord(
            sessionId,
            appointmentId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            TelemedicineSessionMode.Chat,
            1500,
            startedAt,
            endedAt,
            RecordingEnabled: true,
            SummaryText: string.Empty);

        var text = TelemedicineSessionSummaryBuilder.Build(summary);

        Assert.Contains(sessionId.ToString(), text);
        Assert.Contains(appointmentId.ToString(), text);
        Assert.Contains("Mode: Chat", text);
        Assert.Contains("Duration seconds: 1500", text);
        Assert.Contains("Recording enabled: True", text);
    }
}
