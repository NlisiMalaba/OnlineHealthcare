using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.HealthRecords;

public sealed class HealthRecord : Entity
{
    private HealthRecord()
    {
    }

    public Guid PatientId { get; private set; }

    public Guid? ChildProfileId { get; private set; }

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

    public static HealthRecord CreateForChildProfile(Guid guardianPatientId)
    {
        if (guardianPatientId == Guid.Empty)
        {
            throw new ArgumentException("Guardian patient id is required.", nameof(guardianPatientId));
        }

        return new HealthRecord
        {
            PatientId = guardianPatientId
        };
    }

    public void AssignChildProfile(Guid childProfileId)
    {
        if (childProfileId == Guid.Empty)
        {
            throw new ArgumentException("Child profile id is required.", nameof(childProfileId));
        }

        if (ChildProfileId.HasValue)
        {
            throw new InvalidOperationException("Health record is already linked to a child profile.");
        }

        ChildProfileId = childProfileId;
        Touch();
    }
}
