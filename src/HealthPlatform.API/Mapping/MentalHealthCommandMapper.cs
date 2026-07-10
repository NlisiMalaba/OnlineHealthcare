using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.MentalHealth.CompleteTherapySession;
using HealthPlatform.Application.MentalHealth.GrantTherapySessionBroaderAccess;
using HealthPlatform.Application.MentalHealth.MoodLogs.CreateMoodLog;
using HealthPlatform.Application.MentalHealth.MoodLogs.GrantMoodChartSharingConsent;
using HealthPlatform.Application.MentalHealth.MoodLogs.RevokeMoodChartSharingConsent;
using HealthPlatform.Application.MentalHealth.MoodLogs.UpdateMoodLog;

namespace HealthPlatform.API.Mapping;

public static class MentalHealthCommandMapper
{
    public static CompleteTherapySessionCommand ToCompleteCommand(
        Guid therapySessionId,
        CompleteTherapySessionRequest request) =>
        new(therapySessionId, request.SessionSummary);

    public static GrantTherapySessionBroaderAccessCommand ToGrantBroaderAccessCommand(Guid therapySessionId) =>
        new(therapySessionId);

    public static CreateMoodLogCommand ToCreateMoodLogCommand(CreateMoodLogRequest request) =>
        new(request.Rating, request.Notes, request.LoggedAtUtc);

    public static UpdateMoodLogCommand ToUpdateMoodLogCommand(string moodLogId, UpdateMoodLogRequest request) =>
        new(moodLogId, request.Rating, request.Notes, request.LoggedAtUtc);

    public static GrantMoodChartSharingConsentCommand ToGrantMoodChartConsentCommand(
        GrantMoodChartSharingConsentRequest request) =>
        new(request.TherapistId);

    public static RevokeMoodChartSharingConsentCommand ToRevokeMoodChartConsentCommand(Guid therapistId) =>
        new(therapistId);
}
