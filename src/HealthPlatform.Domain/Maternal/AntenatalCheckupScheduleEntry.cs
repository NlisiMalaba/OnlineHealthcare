using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Maternal;

public sealed class AntenatalCheckupScheduleEntry : Entity
{
    private AntenatalCheckupScheduleEntry()
    {
        Description = string.Empty;
    }

    public Guid AntenatalRecordId { get; private set; }

    public int GestationalAgeWeeks { get; private set; }

    public DateOnly RecommendedDate { get; private set; }

    public string Description { get; private set; }

    public static AntenatalCheckupScheduleEntry Create(
        Guid antenatalRecordId,
        int gestationalAgeWeeks,
        DateOnly recommendedDate,
        string description,
        DateTime createdAtUtc)
    {
        if (antenatalRecordId == Guid.Empty)
        {
            throw new ArgumentException("Antenatal record id is required.", nameof(antenatalRecordId));
        }

        if (gestationalAgeWeeks is < 0 or > 42)
        {
            throw new ArgumentOutOfRangeException(
                nameof(gestationalAgeWeeks),
                gestationalAgeWeeks,
                "Gestational age must be between 0 and 42 weeks.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        if (createdAtUtc == default || createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Creation timestamp must be UTC.", nameof(createdAtUtc));
        }

        return new AntenatalCheckupScheduleEntry
        {
            Id = Guid.CreateVersion7(),
            AntenatalRecordId = antenatalRecordId,
            GestationalAgeWeeks = gestationalAgeWeeks,
            RecommendedDate = recommendedDate,
            Description = description.Trim(),
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }
}
