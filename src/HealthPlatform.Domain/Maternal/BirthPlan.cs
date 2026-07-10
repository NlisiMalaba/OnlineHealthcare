using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Maternal.Events;

namespace HealthPlatform.Domain.Maternal;

public sealed class BirthPlan : Entity
{
    private BirthPlan()
    {
        Content = new BirthPlanContent(null, null, null, null);
    }

    public Guid PatientId { get; private set; }

    public Guid AntenatalRecordId { get; private set; }

    public BirthPlanContent Content { get; private set; }

    public static BirthPlan Create(
        Guid patientId,
        Guid antenatalRecordId,
        BirthPlanContent content,
        DateTime createdAtUtc)
    {
        ValidateOwnership(patientId, antenatalRecordId, content, createdAtUtc);

        return new BirthPlan
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            AntenatalRecordId = antenatalRecordId,
            Content = NormalizeContent(content),
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    public void Update(BirthPlanContent content, Guid obstetricDoctorId, DateTime updatedAtUtc)
    {
        if (updatedAtUtc == default || updatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Updated timestamp must be UTC.", nameof(updatedAtUtc));
        }

        if (content.IsEmpty())
        {
            throw new ArgumentException("At least one birth plan preference must be provided.", nameof(content));
        }

        Content = NormalizeContent(content);
        Touch();

        RaiseDomainEvent(new BirthPlanUpdatedDomainEvent(
            Id,
            AntenatalRecordId,
            PatientId,
            obstetricDoctorId,
            updatedAtUtc));
    }

    private static void ValidateOwnership(
        Guid patientId,
        Guid antenatalRecordId,
        BirthPlanContent content,
        DateTime createdAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (antenatalRecordId == Guid.Empty)
        {
            throw new ArgumentException("Antenatal record id is required.", nameof(antenatalRecordId));
        }

        if (content.IsEmpty())
        {
            throw new ArgumentException("At least one birth plan preference must be provided.", nameof(content));
        }

        if (createdAtUtc == default || createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Creation timestamp must be UTC.", nameof(createdAtUtc));
        }
    }

    private static BirthPlanContent NormalizeContent(BirthPlanContent content) =>
        new(
            TrimOrNull(content.LabourPreferences),
            TrimOrNull(content.DeliveryMethod),
            TrimOrNull(content.PainManagement),
            TrimOrNull(content.PostnatalCare));

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
