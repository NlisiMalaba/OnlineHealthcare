using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Maternal;

public sealed class ChildProfile : Entity
{
    private ChildProfile()
    {
        KnownAllergies = [];
    }

    public Guid GuardianId { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public DateOnly DateOfBirth { get; private set; }

    public string? BloodType { get; private set; }

    public IReadOnlyList<string> KnownAllergies { get; private set; }

    public Guid HealthRecordId { get; private set; }

    public static ChildProfile Create(
        Guid guardianId,
        string fullName,
        DateOnly dateOfBirth,
        string? bloodType,
        IReadOnlyCollection<string> knownAllergies,
        Guid healthRecordId,
        DateTime createdAtUtc)
    {
        if (guardianId == Guid.Empty)
        {
            throw new ArgumentException("Guardian id is required.", nameof(guardianId));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (healthRecordId == Guid.Empty)
        {
            throw new ArgumentException("Health record id is required.", nameof(healthRecordId));
        }

        if (createdAtUtc == default || createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Creation timestamp must be UTC.", nameof(createdAtUtc));
        }

        var asOfDate = DateOnly.FromDateTime(createdAtUtc);
        if (dateOfBirth > asOfDate)
        {
            throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));
        }

        var allergies = knownAllergies
            .Select(allergy => allergy.Trim())
            .Where(allergy => allergy.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ChildProfile
        {
            Id = Guid.CreateVersion7(),
            GuardianId = guardianId,
            FullName = fullName.Trim(),
            DateOfBirth = dateOfBirth,
            BloodType = string.IsNullOrWhiteSpace(bloodType) ? null : bloodType.Trim(),
            KnownAllergies = allergies,
            HealthRecordId = healthRecordId,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }
}
