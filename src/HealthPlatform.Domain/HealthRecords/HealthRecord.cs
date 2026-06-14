using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.HealthRecords;

public sealed class HealthRecord : Entity
{
    private HealthRecord()
    {
    }

    public Guid PatientId { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public static HealthRecord CreateForPatient(Guid patientId)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        return new HealthRecord
        {
            PatientId = patientId
        };
    }
}
