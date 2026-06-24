using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Telemedicine.Notifications;
using HealthPlatform.Domain.Telemedicine;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Telemedicine.EventHandlers;

public sealed class AttachTelemedicineSessionSummaryOnSessionEndedNotificationHandler(
    IHealthRecordRepository healthRecordRepository,
    ITelemedicineSessionRepository telemedicineSessionRepository,
    ITelemedicineSessionSummaryRepository summaryRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    ILogger<AttachTelemedicineSessionSummaryOnSessionEndedNotificationHandler> logger)
    : INotificationHandler<TelemedicineSessionEndedNotification>
{
    public async Task Handle(TelemedicineSessionEndedNotification notification, CancellationToken ct)
    {
        var session = await telemedicineSessionRepository.GetByIdAsync(notification.SessionId, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.SessionNotFound,
                "Telemedicine session was not found.");

        if (!string.IsNullOrWhiteSpace(session.SessionSummaryRef))
        {
            return;
        }

        var healthRecord = await healthRecordRepository.GetByPatientIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordNotFound,
                "Health record was not found.");

        var summaryText = TelemedicineSessionSummaryBuilder.Build(
            new TelemedicineSessionSummaryRecord(
                notification.SessionId,
                notification.AppointmentId,
                notification.PatientId,
                notification.DoctorId,
                notification.Mode,
                notification.DurationSeconds,
                notification.StartedAtUtc,
                notification.EndedAtUtc,
                notification.RecordingEnabled,
                string.Empty));

        var summaryRecord = new TelemedicineSessionSummaryRecord(
            notification.SessionId,
            notification.AppointmentId,
            notification.PatientId,
            notification.DoctorId,
            notification.Mode,
            notification.DurationSeconds,
            notification.StartedAtUtc,
            notification.EndedAtUtc,
            notification.RecordingEnabled,
            summaryText);

        var summaryReference = await summaryRepository.SaveAsync(summaryRecord, ct);

        await healthRecordEntryRepository.AddTelemedicineSessionSummaryEntryAsync(
            new HealthRecordTelemedicineSummaryEntry(
                healthRecord.Id,
                notification.PatientId,
                notification.DoctorId,
                notification.SessionId,
                notification.AppointmentId,
                summaryReference.DocumentId,
                notification.OccurredAtUtc),
            ct);

        session.AttachSessionSummary(summaryReference.DocumentId);
        await telemedicineSessionRepository.UpdateAsync(session, ct);

        logger.LogInformation(
            "Attached telemedicine session summary {SummaryDocumentId} to health record {HealthRecordId}.",
            summaryReference.DocumentId,
            healthRecord.Id);
    }
}
