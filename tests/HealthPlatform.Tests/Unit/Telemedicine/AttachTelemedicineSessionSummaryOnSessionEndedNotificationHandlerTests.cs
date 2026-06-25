using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.EventHandlers;
using HealthPlatform.Application.Telemedicine.Notifications;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Infrastructure.MongoDb;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class AttachTelemedicineSessionSummaryOnSessionEndedNotificationHandlerTests
{
    [Fact]
    public async Task Handle_persists_summary_and_links_health_record()
    {
        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var session = CreateEndedSession(appointmentId);
        var healthRecord = HealthRecord.CreateForPatient(patientId);
        var summaryRepository = new InMemoryTelemedicineSessionSummaryRepository();
        var entryRepository = new InMemoryHealthRecordEntryRepository();

        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        healthRecordRepository
            .Setup(repo => repo.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthRecord);

        var sessionRepository = new Mock<ITelemedicineSessionRepository>();
        sessionRepository
            .Setup(repo => repo.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        sessionRepository
            .Setup(repo => repo.UpdateAsync(session, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new AttachTelemedicineSessionSummaryOnSessionEndedNotificationHandler(
            healthRecordRepository.Object,
            sessionRepository.Object,
            summaryRepository,
            entryRepository,
            NullLogger<AttachTelemedicineSessionSummaryOnSessionEndedNotificationHandler>.Instance);

        var startedAt = DateTime.UtcNow.AddMinutes(-20);
        var endedAt = DateTime.UtcNow;

        await handler.Handle(
            new TelemedicineSessionEndedNotification(
                session.Id,
                appointmentId,
                patientId,
                doctorId,
                TelemedicineSessionMode.Audio,
                1200,
                startedAt,
                endedAt,
                RecordingEnabled: false,
                endedAt),
            CancellationToken.None);

        Assert.Single(summaryRepository.Summaries);
        Assert.Equal(TelemedicineSessionMode.Audio, summaryRepository.Summaries[0].Mode);
        Assert.Single(entryRepository.Entries);
        Assert.Equal(healthRecord.Id, entryRepository.Entries[0].HealthRecordId);
        Assert.Equal(session.Id, entryRepository.Entries[0].SessionId);
        Assert.NotNull(session.SessionSummaryRef);
        Assert.Equal(entryRepository.Entries[0].SummaryDocumentId, session.SessionSummaryRef);

        sessionRepository.Verify(
            repo => repo.UpdateAsync(session, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_skips_when_summary_already_attached()
    {
        var patientId = Guid.CreateVersion7();
        var session = CreateEndedSession(Guid.CreateVersion7());
        session.AttachSessionSummary("existing-summary-id");

        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        var sessionRepository = new Mock<ITelemedicineSessionRepository>();
        sessionRepository
            .Setup(repo => repo.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var summaryRepository = new InMemoryTelemedicineSessionSummaryRepository();
        var entryRepository = new InMemoryHealthRecordEntryRepository();

        var handler = new AttachTelemedicineSessionSummaryOnSessionEndedNotificationHandler(
            healthRecordRepository.Object,
            sessionRepository.Object,
            summaryRepository,
            entryRepository,
            NullLogger<AttachTelemedicineSessionSummaryOnSessionEndedNotificationHandler>.Instance);

        await handler.Handle(
            new TelemedicineSessionEndedNotification(
                session.Id,
                session.AppointmentId,
                patientId,
                Guid.CreateVersion7(),
                TelemedicineSessionMode.Video,
                600,
                DateTime.UtcNow.AddMinutes(-10),
                DateTime.UtcNow,
                RecordingEnabled: false,
                DateTime.UtcNow),
            CancellationToken.None);

        Assert.Empty(summaryRepository.Summaries);
        Assert.Empty(entryRepository.Entries);
        healthRecordRepository.Verify(
            repo => repo.GetByPatientIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static TelemedicineSession CreateEndedSession(Guid appointmentId)
    {
        var session = TelemedicineSession.CreateForAppointment(appointmentId, RtcProvider.Twilio);
        session.MarkJoined(DateTime.UtcNow.AddMinutes(-15), TelemedicineSessionMode.Video);
        session.End(Guid.CreateVersion7(), Guid.CreateVersion7(), DateTime.UtcNow);
        return session;
    }
}
