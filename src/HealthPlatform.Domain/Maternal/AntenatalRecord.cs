using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Maternal.Events;

namespace HealthPlatform.Domain.Maternal;

public sealed class AntenatalRecord : Entity
{
    private AntenatalRecord()
    {
        EntryRefs = [];
    }

    public Guid PatientId { get; private set; }

    public DateOnly EstimatedDueDate { get; private set; }

    public int GestationalAgeWeeks { get; private set; }

    public Guid ObstetricDoctorId { get; private set; }

    public AntenatalRecordStatus Status { get; private set; }

    public IReadOnlyList<string> EntryRefs { get; private set; }

    public DateTime? NextReminderAtUtc { get; private set; }

    public DateTime? LastReminderSentAtUtc { get; private set; }

    public static AntenatalRecord Create(
        Guid patientId,
        DateOnly estimatedDueDate,
        int gestationalAgeWeeks,
        Guid obstetricDoctorId,
        DateTime createdAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (obstetricDoctorId == Guid.Empty)
        {
            throw new ArgumentException("Obstetric doctor id is required.", nameof(obstetricDoctorId));
        }

        if (gestationalAgeWeeks is < 0 or > 42)
        {
            throw new ArgumentOutOfRangeException(
                nameof(gestationalAgeWeeks),
                gestationalAgeWeeks,
                "Gestational age must be between 0 and 42 weeks.");
        }

        if (createdAtUtc == default || createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Creation timestamp must be UTC.", nameof(createdAtUtc));
        }

        var asOfDate = DateOnly.FromDateTime(createdAtUtc);
        if (estimatedDueDate < asOfDate)
        {
            throw new ArgumentException(
                "Estimated due date cannot be in the past.",
                nameof(estimatedDueDate));
        }

        var record = new AntenatalRecord
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            EstimatedDueDate = estimatedDueDate,
            GestationalAgeWeeks = gestationalAgeWeeks,
            ObstetricDoctorId = obstetricDoctorId,
            Status = AntenatalRecordStatus.Active,
            EntryRefs = [],
            NextReminderAtUtc = AntenatalReminderPolicies.CalculateNextReminderAtUtc(
                estimatedDueDate,
                createdAtUtc),
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };

        record.RaiseDomainEvent(new AntenatalRecordCreatedDomainEvent(
            record.Id,
            record.PatientId,
            record.ObstetricDoctorId,
            record.EstimatedDueDate,
            record.GestationalAgeWeeks,
            createdAtUtc));

        return record;
    }

    public bool MarkReminderSent(DateTime sentAtUtc)
    {
        if (Status != AntenatalRecordStatus.Active)
        {
            return false;
        }

        if (sentAtUtc == default || sentAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Sent timestamp must be UTC.", nameof(sentAtUtc));
        }

        if (NextReminderAtUtc.HasValue && sentAtUtc < NextReminderAtUtc.Value)
        {
            return false;
        }

        LastReminderSentAtUtc = sentAtUtc;
        NextReminderAtUtc = AntenatalReminderPolicies.CalculateNextReminderAtUtc(
            EstimatedDueDate,
            sentAtUtc);
        Touch();
        return true;
    }
}
