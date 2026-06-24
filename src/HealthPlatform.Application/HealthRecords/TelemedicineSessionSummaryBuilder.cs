using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.Application.HealthRecords;

public static class TelemedicineSessionSummaryBuilder
{
    public static string Build(TelemedicineSessionSummaryRecord summary) =>
        $"""
        Telemedicine consultation summary
        Appointment: {summary.AppointmentId}
        Session: {summary.SessionId}
        Mode: {summary.Mode}
        Duration seconds: {summary.DurationSeconds}
        Started at (UTC): {summary.StartedAtUtc:O}
        Ended at (UTC): {summary.EndedAtUtc:O}
        Recording enabled: {summary.RecordingEnabled}
        """;
}
