using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Vaccinations;

public sealed class VaccinationRecord : Entity
{
    private VaccinationRecord()
    {
        VaccineName = string.Empty;
        BatchNumber = string.Empty;
        Provider = string.Empty;
    }

    public Guid? ChildProfileId { get; private set; }

    public Guid? PatientId { get; private set; }

    public Guid? ScheduleEntryId { get; private set; }

    public string VaccineName { get; private set; }

    public DateOnly AdministeredDate { get; private set; }

    public string BatchNumber { get; private set; }

    public string Provider { get; private set; }

    public Guid RecordedByUserId { get; private set; }

    public static VaccinationRecord CreateForChild(
        Guid childProfileId,
        Guid? scheduleEntryId,
        string vaccineName,
        DateOnly administeredDate,
        string batchNumber,
        string provider,
        Guid recordedByUserId,
        DateTime createdAtUtc)
    {
        ValidateCreate(
            childProfileId,
            null,
            scheduleEntryId,
            vaccineName,
            administeredDate,
            batchNumber,
            provider,
            recordedByUserId,
            createdAtUtc);

        return new VaccinationRecord
        {
            Id = Guid.CreateVersion7(),
            ChildProfileId = childProfileId,
            ScheduleEntryId = scheduleEntryId,
            VaccineName = vaccineName.Trim(),
            AdministeredDate = administeredDate,
            BatchNumber = batchNumber.Trim(),
            Provider = provider.Trim(),
            RecordedByUserId = recordedByUserId,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    public static VaccinationRecord CreateForPatient(
        Guid patientId,
        Guid? scheduleEntryId,
        string vaccineName,
        DateOnly administeredDate,
        string batchNumber,
        string provider,
        Guid recordedByUserId,
        DateTime createdAtUtc)
    {
        ValidateCreate(
            null,
            patientId,
            scheduleEntryId,
            vaccineName,
            administeredDate,
            batchNumber,
            provider,
            recordedByUserId,
            createdAtUtc);

        return new VaccinationRecord
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            ScheduleEntryId = scheduleEntryId,
            VaccineName = vaccineName.Trim(),
            AdministeredDate = administeredDate,
            BatchNumber = batchNumber.Trim(),
            Provider = provider.Trim(),
            RecordedByUserId = recordedByUserId,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    private static void ValidateCreate(
        Guid? childProfileId,
        Guid? patientId,
        Guid? scheduleEntryId,
        string vaccineName,
        DateOnly administeredDate,
        string batchNumber,
        string provider,
        Guid recordedByUserId,
        DateTime createdAtUtc)
    {
        var hasChild = childProfileId.HasValue && childProfileId != Guid.Empty;
        var hasPatient = patientId.HasValue && patientId != Guid.Empty;

        if (hasChild == hasPatient)
        {
            throw new ArgumentException("Exactly one of child profile id or patient id is required.");
        }

        if (scheduleEntryId == Guid.Empty)
        {
            throw new ArgumentException("Schedule entry id cannot be empty when provided.", nameof(scheduleEntryId));
        }

        if (string.IsNullOrWhiteSpace(vaccineName))
        {
            throw new ArgumentException("Vaccine name is required.", nameof(vaccineName));
        }

        if (string.IsNullOrWhiteSpace(batchNumber))
        {
            throw new ArgumentException("Batch number is required.", nameof(batchNumber));
        }

        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentException("Provider is required.", nameof(provider));
        }

        if (recordedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Recorded by user id is required.", nameof(recordedByUserId));
        }

        if (createdAtUtc == default || createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Creation timestamp must be UTC.", nameof(createdAtUtc));
        }
    }
}
