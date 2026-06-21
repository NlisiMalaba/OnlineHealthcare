using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity;

public interface ILicenseVerificationQueueRepository
{
    Task EnqueueAsync(LicenseVerificationQueueItem item, CancellationToken ct);

    Task<bool> ExistsPendingForDoctorAsync(Guid doctorId, CancellationToken ct);
}
