using HealthPlatform.Application.Labs;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Labs;

public sealed class LoggingLabPartnerOrderClient(ILogger<LoggingLabPartnerOrderClient> logger) : ILabPartnerOrderClient
{
    public Task<string> SubmitOrderAsync(LabPartnerOrderSubmission submission, CancellationToken ct)
    {
        logger.LogInformation(
            "Submitting lab order {LabOrderId} to partner {LabPartnerCode} for test {TestCode}",
            submission.LabOrderId,
            submission.LabPartnerCode,
            submission.TestCode);

        return Task.FromResult($"{submission.LabPartnerCode}-{submission.LabOrderId:N}");
    }
}
