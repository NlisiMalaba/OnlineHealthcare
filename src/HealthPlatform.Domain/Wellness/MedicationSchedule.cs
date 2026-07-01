using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Wellness;

public sealed class MedicationSchedule : Entity
{
    private readonly List<DateTime> _doseTimes = [];

    private MedicationSchedule()
    {
        MedicationName = string.Empty;
    }

    public Guid PrescriptionId { get; private set; }

    public Guid PatientId { get; private set; }

    public string MedicationName { get; private set; }

    public IReadOnlyList<DateTime> DoseTimes => _doseTimes;

    public MedicationScheduleStatus Status { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public static MedicationSchedule CreateActive(
        Guid prescriptionId,
        Guid patientId,
        string medicationName,
        IReadOnlyList<DateTime> doseTimes)
    {
        if (prescriptionId == Guid.Empty)
        {
            throw new ArgumentException("Prescription id is required.", nameof(prescriptionId));
        }

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (string.IsNullOrWhiteSpace(medicationName))
        {
            throw new ArgumentException("Medication name is required.", nameof(medicationName));
        }

        if (doseTimes.Count == 0)
        {
            throw new ArgumentException("At least one dose time is required.", nameof(doseTimes));
        }

        if (doseTimes.Any(doseTime => doseTime.Kind != DateTimeKind.Utc))
        {
            throw new ArgumentException("Dose times must be UTC.", nameof(doseTimes));
        }

        var schedule = new MedicationSchedule
        {
            Id = Guid.CreateVersion7(),
            PrescriptionId = prescriptionId,
            PatientId = patientId,
            MedicationName = medicationName.Trim(),
            Status = MedicationScheduleStatus.Active
        };

        schedule._doseTimes.AddRange(doseTimes.OrderBy(doseTime => doseTime));
        return schedule;
    }

    public bool MarkCompleted(DateTime completedAtUtc)
    {
        if (completedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Completion time must be UTC.", nameof(completedAtUtc));
        }

        if (Status == MedicationScheduleStatus.Completed)
        {
            return false;
        }

        Status = MedicationScheduleStatus.Completed;
        CompletedAtUtc = completedAtUtc;
        return true;
    }
}
