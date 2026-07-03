using HealthPlatform.Application.Telemedicine.EndSession;
using HealthPlatform.Application.Telemedicine.Notifications;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Infrastructure.MongoDb;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class EndTelemedicineSessionCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Handle_ends_session_and_attaches_summary_to_health_record()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateActiveAsync(_host);
        _host.CurrentUser.UserId = context.DoctorUserId;

        var response = await _host.Sender.Send(
            new EndTelemedicineSessionCommand(context.AppointmentId),
            CancellationToken.None);

        Assert.Equal("ended", response.Status);
        Assert.True(response.DurationSeconds >= 0);

        var session = await _host.DbContext.TelemedicineSessions.SingleAsync();
        Assert.Equal(TelemedicineSessionStatus.Ended, session.Status);
        Assert.NotNull(session.SessionSummaryRef);

        var summaryRepository = _host.GetRequiredService<InMemoryTelemedicineSessionSummaryRepository>();
        var entryRepository = _host.GetRequiredService<InMemoryHealthRecordEntryRepository>();
        Assert.Single(summaryRepository.Summaries);
        Assert.Single(entryRepository.Entries);
        Assert.Equal(
            session.SessionSummaryRef,
            entryRepository.Entries[0].Content.TelemedicineSessionSummary!.SummaryDocumentId);

        var hasOutboxEvent = await _host.DbContext.DomainEventOutbox
            .AsNoTracking()
            .AnyAsync(x => x.EventType.Contains("TelemedicineSessionEndedDomainEvent"));

        Assert.True(hasOutboxEvent);
    }

    [Fact]
    public async Task Handle_includes_mode_and_duration_in_persisted_summary()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateActiveAsync(_host);
        _host.CurrentUser.UserId = context.PatientUserId;

        await _host.Sender.Send(
            new Application.Telemedicine.JoinSession.JoinTelemedicineSessionCommand(
                context.AppointmentId,
                TelemedicineSessionMode.Chat),
            CancellationToken.None);

        _host.CurrentUser.UserId = context.DoctorUserId;

        await _host.Sender.Send(
            new EndTelemedicineSessionCommand(context.AppointmentId),
            CancellationToken.None);

        var summaryRepository = _host.GetRequiredService<InMemoryTelemedicineSessionSummaryRepository>();
        var summary = Assert.Single(summaryRepository.Summaries);

        Assert.Equal(TelemedicineSessionMode.Chat, summary.Mode);
        Assert.True(summary.DurationSeconds >= 0);
        Assert.Contains("Mode: Chat", summary.SummaryText);
    }
}
