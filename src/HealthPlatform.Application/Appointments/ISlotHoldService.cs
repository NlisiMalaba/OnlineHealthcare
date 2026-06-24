namespace HealthPlatform.Application.Appointments;

public interface ISlotHoldService
{
    Task<bool> TryHoldAsync(Guid slotId, Guid patientId, TimeSpan ttl, CancellationToken ct);
}
