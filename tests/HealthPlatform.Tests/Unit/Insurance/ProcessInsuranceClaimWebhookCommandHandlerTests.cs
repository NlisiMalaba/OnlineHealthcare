using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Insurance;
using HealthPlatform.Application.Insurance.Webhooks;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Infrastructure.Insurance;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Insurance;

public sealed class ProcessInsuranceClaimWebhookCommandHandlerTests
{
    [Fact]
    public async Task Handler_rejects_invalid_signature()
    {
        var insurerClient = new Mock<IInsurerApiClient>();
        insurerClient.Setup(c => c.InsurerCode).Returns("demo-insurer");
        insurerClient.Setup(c => c.ParseStatusWebhookAsync(It.IsAny<InsurerWebhookParseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InsurerWebhookParseResult(false, null, null, null, null));

        var resolver = new Mock<IInsurerApiClientResolver>();
        resolver.Setup(r => r.GetRequired("demo-insurer")).Returns(insurerClient.Object);

        var handler = new ProcessInsuranceClaimWebhookCommandHandler(
            resolver.Object,
            new Mock<IInsuranceClaimRepository>().Object,
            new InMemoryInsuranceClaimWebhookIdempotencyStore(),
            new Mock<IOutboxRepository>().Object,
            new Mock<IDomainEventPublisher>().Object,
            TimeProvider.System,
            NullLogger<ProcessInsuranceClaimWebhookCommandHandler>.Instance);

        await Assert.ThrowsAsync<AccessDeniedException>(() => handler.Handle(
            new ProcessInsuranceClaimWebhookCommand("demo-insurer", "{}", new Dictionary<string, string>()),
            CancellationToken.None));
    }
}
