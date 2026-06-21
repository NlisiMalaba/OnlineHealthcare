namespace HealthPlatform.Domain.HealthRecords;

public sealed class HealthRecordProfileChange
{
    private HealthRecordProfileChange()
    {
        FieldName = string.Empty;
    }

    public Guid Id { get; private set; }

    public Guid HealthRecordId { get; private set; }

    public Guid PatientId { get; private set; }

    public string FieldName { get; private set; }

    public string? PreviousValue { get; private set; }

    public string? NewValue { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }

    public static HealthRecordProfileChange Create(
        Guid healthRecordId,
        Guid patientId,
        string fieldName,
        string? previousValue,
        string? newValue,
        DateTime changedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        return new HealthRecordProfileChange
        {
            Id = Guid.CreateVersion7(),
            HealthRecordId = healthRecordId,
            PatientId = patientId,
            FieldName = fieldName,
            PreviousValue = previousValue,
            NewValue = newValue,
            ChangedAtUtc = changedAtUtc
        };
    }
}
