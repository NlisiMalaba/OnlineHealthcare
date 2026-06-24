using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Wellness;

public sealed class MedicationSchedule : Entity
{
    private MedicationSchedule()
    {
        MedicationName = string.Empty;
    }

    public Guid PrescriptionId { get; private set; }

    public Guid PatientId { get; private set; }

    public string MedicationName { get; private set; }

    public MedicationScheduleStatus Status { get; private set; }

    public static MedicationSchedule CreateActive(
        Guid prescriptionId,
        Guid patientId,
        string medicationName)
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

        return new MedicationSchedule
        {
            Id = Guid.CreateVersion7(),
            PrescriptionId = prescriptionId,
            PatientId = patientId,
            MedicationName = medicationName.Trim(),
            Status = MedicationScheduleStatus.Active
        };
    }
}
