namespace HealthPlatform.Application.Appointments;

public interface ISlotHoldService
{
    Task<bool> TryHoldAsync(Guid slotId, Guid patientId, TimeSpan ttl, CancellationToken ct);

    Task ReleaseHoldAsync(Guid slotId, CancellationToken ct);

    Task ExtendHoldAsync(Guid slotId, TimeSpan ttl, CancellationToken ct);

    Task<bool> IsSlotHeldAsync(Guid slotId, CancellationToken ct);
}
