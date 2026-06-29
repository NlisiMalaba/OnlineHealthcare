using HealthPlatform.Application.Insurance;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

public sealed class InsuranceClaimStatusPollingJob(
    IInsuranceClaimStatusPoller statusPoller,
    ILogger<InsuranceClaimStatusPollingJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var updated = await statusPoller.PollPendingClaimsAsync(ct);
        if (updated == 0)
        {
            logger.LogDebug("Insurance claim status polling tick — no claim updates.");
        }
    }

    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
