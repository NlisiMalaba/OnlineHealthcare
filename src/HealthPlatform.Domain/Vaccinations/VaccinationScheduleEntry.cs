using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Vaccinations;

public sealed class VaccinationScheduleEntry : Entity
{
    private VaccinationScheduleEntry()
    {
        VaccineName = string.Empty;
        Description = string.Empty;
    }

    public Guid? ChildProfileId { get; private set; }

    public Guid? PatientId { get; private set; }

    public string VaccineName { get; private set; }

    public string Description { get; private set; }

    public DateOnly RecommendedDate { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public Guid? VaccinationRecordId { get; private set; }

    public DateTime? ReminderSentAtUtc { get; private set; }

    public static VaccinationScheduleEntry CreateForChild(
        Guid childProfileId,
        string vaccineName,
        DateOnly recommendedDate,
        string description,
        DateTime createdAtUtc)
    {
        ValidateSubject(childProfileId, null, vaccineName, recommendedDate, description, createdAtUtc);

        return new VaccinationScheduleEntry
        {
            Id = Guid.CreateVersion7(),
            ChildProfileId = childProfileId,
            VaccineName = vaccineName.Trim(),
            Description = description.Trim(),
            RecommendedDate = recommendedDate,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    public static VaccinationScheduleEntry CreateForPatient(
        Guid patientId,
        string vaccineName,
        DateOnly recommendedDate,
        string description,
        DateTime createdAtUtc)
    {
        ValidateSubject(null, patientId, vaccineName, recommendedDate, description, createdAtUtc);

        return new VaccinationScheduleEntry
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            VaccineName = vaccineName.Trim(),
            Description = description.Trim(),
            RecommendedDate = recommendedDate,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    public bool MarkReminderSent(DateTime sentAtUtc)
    {
        if (ReminderSentAtUtc.HasValue || CompletedAtUtc.HasValue)
        {
            return false;
        }

        if (sentAtUtc == default || sentAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Reminder timestamp must be UTC.", nameof(sentAtUtc));
        }

        ReminderSentAtUtc = sentAtUtc;
        Touch();
        return true;
    }

    public void MarkCompleted(Guid vaccinationRecordId, DateTime completedAtUtc)
    {
        if (CompletedAtUtc.HasValue)
        {
            throw new InvalidOperationException("Vaccination schedule entry is already completed.");
        }

        if (vaccinationRecordId == Guid.Empty)
        {
            throw new ArgumentException("Vaccination record id is required.", nameof(vaccinationRecordId));
        }

        if (completedAtUtc == default || completedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Completion timestamp must be UTC.", nameof(completedAtUtc));
        }

        VaccinationRecordId = vaccinationRecordId;
        CompletedAtUtc = completedAtUtc;
        Touch();
    }

    private static void ValidateSubject(
        Guid? childProfileId,
        Guid? patientId,
        string vaccineName,
        DateOnly recommendedDate,
        string description,
        DateTime createdAtUtc)
    {
        var hasChild = childProfileId.HasValue && childProfileId != Guid.Empty;
        var hasPatient = patientId.HasValue && patientId != Guid.Empty;

        if (hasChild == hasPatient)
        {
            throw new ArgumentException("Exactly one of child profile id or patient id is required.");
        }

        if (string.IsNullOrWhiteSpace(vaccineName))
        {
            throw new ArgumentException("Vaccine name is required.", nameof(vaccineName));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        if (createdAtUtc == default || createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Creation timestamp must be UTC.", nameof(createdAtUtc));
        }
    }
}
