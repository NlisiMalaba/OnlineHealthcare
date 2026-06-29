using HealthPlatform.Application.Insurance;
using HealthPlatform.Application.Insurance.Webhooks;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Insurance;
using HealthPlatform.Infrastructure.Insurance;
using HealthPlatform.Infrastructure.Persistence.Repositories;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HealthPlatform.Tests.Unit.Insurance;

public sealed class InsuranceClaimStatusPollerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task PollPendingClaims_updates_submitted_claim_status()
    {
        var patientId = Guid.CreateVersion7();
        var claim = InsuranceClaim.Create(
            patientId,
            Guid.CreateVersion7(),
            "demo-insurer",
            InsuranceClaimType.Consultation,
            2500,
            "USD",
            Guid.CreateVersion7(),
            null,
            null);
        claim.MarkSubmitted("dev_demo-insurer_poll", DateTime.UtcNow);

        _host.DbContext.InsuranceClaims.Add(claim);
        await _host.DbContext.SaveChangesAsync();

        var poller = new InsuranceClaimStatusPoller(
            new InsuranceClaimRepository(_host.DbContext),
            _host.GetRequiredService<IInsurerApiClientResolver>(),
            _host.GetRequiredService<IOutboxRepository>(),
            _host.GetRequiredService<IDomainEventPublisher>(),
            TimeProvider.System,
            NullLogger<InsuranceClaimStatusPoller>.Instance);

        var updated = await poller.PollPendingClaimsAsync(CancellationToken.None);

        Assert.Equal(1, updated);

        var refreshed = await _host.DbContext.InsuranceClaims.SingleAsync(c => c.Id == claim.Id);
        Assert.Equal(InsuranceClaimStatus.Processing, refreshed.Status);
        Assert.NotNull(refreshed.LastStatusCheckedAtUtc);
    }
}
