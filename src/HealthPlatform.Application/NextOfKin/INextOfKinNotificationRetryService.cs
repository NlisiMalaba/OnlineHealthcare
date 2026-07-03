namespace HealthPlatform.Application.NextOfKin;

public interface INextOfKinNotificationRetryService
{
    Task<int> ProcessDueRetriesAsync(CancellationToken ct);
}
