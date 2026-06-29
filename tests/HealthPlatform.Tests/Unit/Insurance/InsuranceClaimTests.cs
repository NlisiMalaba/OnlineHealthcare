using HealthPlatform.Domain.Insurance;
using Xunit;

namespace HealthPlatform.Tests.Unit.Insurance;

public sealed class InsuranceClaimTests
{
    [Fact]
    public void Create_sets_draft_status_and_target()
    {
        var appointmentId = Guid.CreateVersion7();
        var claim = InsuranceClaim.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "demo-insurer",
            InsuranceClaimType.Consultation,
            2500,
            "USD",
            appointmentId,
            null,
            null);

        Assert.Equal(InsuranceClaimStatus.Draft, claim.Status);
        Assert.Equal(appointmentId, claim.AppointmentId);
    }

    [Fact]
    public void MarkSubmitted_transitions_to_submitted_and_raises_event()
    {
        var claim = CreateConsultationClaim();
        claim.MarkSubmitted("ins-ref-1", DateTime.UtcNow);

        Assert.Equal(InsuranceClaimStatus.Submitted, claim.Status);
        Assert.Equal("ins-ref-1", claim.InsurerClaimReference);
        Assert.Single(claim.DomainEvents);
    }

    [Fact]
    public void TryUpdateStatus_allows_processing_to_approved()
    {
        var claim = CreateConsultationClaim();
        claim.MarkSubmitted("ins-ref-1", DateTime.UtcNow);
        claim.TryUpdateStatus(InsuranceClaimStatus.Processing, null, DateTime.UtcNow);

        var changed = claim.TryUpdateStatus(
            InsuranceClaimStatus.Approved,
            "Covered",
            DateTime.UtcNow);

        Assert.True(changed);
        Assert.Equal(InsuranceClaimStatus.Approved, claim.Status);
        Assert.Equal("Covered", claim.StatusReason);
    }

    private static InsuranceClaim CreateConsultationClaim() =>
        InsuranceClaim.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "demo-insurer",
            InsuranceClaimType.Consultation,
            2500,
            "USD",
            Guid.CreateVersion7(),
            null,
            null);
}
