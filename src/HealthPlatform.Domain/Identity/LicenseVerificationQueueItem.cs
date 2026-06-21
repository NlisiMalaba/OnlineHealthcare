using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Identity;

public sealed class LicenseVerificationQueueItem : Entity
{
    private LicenseVerificationQueueItem()
    {
    }

    public Guid DoctorId { get; private set; }

    public DateTime QueuedAtUtc { get; private set; }

    public bool IsCompleted { get; private set; }

    public static LicenseVerificationQueueItem Create(Guid doctorId)
    {
        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("Doctor id is required.", nameof(doctorId));
        }

        return new LicenseVerificationQueueItem
        {
            DoctorId = doctorId,
            QueuedAtUtc = DateTime.UtcNow,
            IsCompleted = false
        };
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
        Touch();
    }
}
