using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Maternal;

public sealed class GrowthEntry : Entity
{
    private GrowthEntry()
    {
    }

    public Guid ChildProfileId { get; private set; }

    public decimal? HeightCm { get; private set; }

    public decimal? WeightKg { get; private set; }

    public string? MilestoneNote { get; private set; }

    public DateTime RecordedAtUtc { get; private set; }

    public static GrowthEntry Create(
        Guid childProfileId,
        decimal? heightCm,
        decimal? weightKg,
        string? milestoneNote,
        DateTime recordedAtUtc,
        DateTime createdAtUtc)
    {
        if (childProfileId == Guid.Empty)
        {
            throw new ArgumentException("Child profile id is required.", nameof(childProfileId));
        }

        if (!heightCm.HasValue && !weightKg.HasValue && string.IsNullOrWhiteSpace(milestoneNote))
        {
            throw new ArgumentException("At least one measurement or milestone note is required.");
        }

        if (heightCm is <= 0)
        {
            throw new ArgumentException("Height must be positive when provided.", nameof(heightCm));
        }

        if (weightKg is <= 0)
        {
            throw new ArgumentException("Weight must be positive when provided.", nameof(weightKg));
        }

        if (recordedAtUtc == default || recordedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Recorded timestamp must be UTC.", nameof(recordedAtUtc));
        }

        if (createdAtUtc == default || createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Creation timestamp must be UTC.", nameof(createdAtUtc));
        }

        return new GrowthEntry
        {
            Id = Guid.CreateVersion7(),
            ChildProfileId = childProfileId,
            HeightCm = heightCm,
            WeightKg = weightKg,
            MilestoneNote = string.IsNullOrWhiteSpace(milestoneNote) ? null : milestoneNote.Trim(),
            RecordedAtUtc = recordedAtUtc,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }
}
