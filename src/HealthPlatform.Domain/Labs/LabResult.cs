using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Labs;

public sealed class LabResult : Entity
{
    private LabResult()
    {
        LabPartnerCode = string.Empty;
        LabPartnerOrderReference = string.Empty;
        StorageKey = string.Empty;
        ContentType = string.Empty;
        FileName = string.Empty;
        TestCode = string.Empty;
    }

    public Guid LabOrderId { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid HealthRecordId { get; private set; }

    public Guid? OrderingDoctorId { get; private set; }

    public string LabPartnerCode { get; private set; }

    public string LabPartnerOrderReference { get; private set; }

    public string TestCode { get; private set; }

    public string StorageKey { get; private set; }

    public string ContentType { get; private set; }

    public string FileName { get; private set; }

    public bool IsCritical { get; private set; }

    public static LabResult Create(
        Guid labOrderId,
        Guid patientId,
        Guid healthRecordId,
        Guid? orderingDoctorId,
        string labPartnerCode,
        string labPartnerOrderReference,
        string testCode,
        string storageKey,
        string contentType,
        string fileName,
        bool isCritical)
    {
        if (labOrderId == Guid.Empty)
        {
            throw new ArgumentException("Lab order id is required.", nameof(labOrderId));
        }

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (healthRecordId == Guid.Empty)
        {
            throw new ArgumentException("Health record id is required.", nameof(healthRecordId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(labPartnerCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(labPartnerOrderReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        return new LabResult
        {
            LabOrderId = labOrderId,
            PatientId = patientId,
            HealthRecordId = healthRecordId,
            OrderingDoctorId = orderingDoctorId,
            LabPartnerCode = labPartnerCode.Trim().ToUpperInvariant(),
            LabPartnerOrderReference = labPartnerOrderReference.Trim(),
            TestCode = testCode.Trim().ToUpperInvariant(),
            StorageKey = storageKey.Trim(),
            ContentType = contentType.Trim(),
            FileName = fileName.Trim(),
            IsCritical = isCritical
        };
    }
}
