using HealthPlatform.Application.NextOfKin;

namespace HealthPlatform.Tests.Support;

public sealed class ControllableNextOfKinChannelDeliveryGateway : INextOfKinChannelDeliveryGateway
{
    public bool SucceedOnRetry { get; set; }

    public int? SucceedOnAttempt { get; set; }

    public int AttemptCount { get; private set; }

    public Task<bool> TryDeliverEmergencyAlertAsync(
        EmergencyAlertChannelDeliveryRequest request,
        CancellationToken ct)
    {
        AttemptCount++;
        var succeeded = SucceedOnAttempt.HasValue
            ? AttemptCount >= SucceedOnAttempt.Value
            : SucceedOnRetry;
        return Task.FromResult(succeeded);
    }
}
