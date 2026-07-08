using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Labs;

public sealed class RadiologyReport : Entity
{
    private RadiologyReport()
    {
        LabPartnerCode = string.Empty;
        LabPartnerOrderReference = string.Empty;
        ReportStorageKey = string.Empty;
        ReportContentType = string.Empty;
        ReportFileName = string.Empty;
        ImagingStorageKeysJson = "[]";
    }

    public Guid LabOrderId { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid HealthRecordId { get; private set; }

    public Guid? OrderingDoctorId { get; private set; }

    public string LabPartnerCode { get; private set; }

    public string LabPartnerOrderReference { get; private set; }

    public string ReportStorageKey { get; private set; }

    public string ReportContentType { get; private set; }

    public string ReportFileName { get; private set; }

    public string ImagingStorageKeysJson { get; private set; }

    public static RadiologyReport Create(
        Guid labOrderId,
        Guid patientId,
        Guid healthRecordId,
        Guid? orderingDoctorId,
        string labPartnerCode,
        string labPartnerOrderReference,
        string reportStorageKey,
        string reportContentType,
        string reportFileName,
        IReadOnlyList<string> imagingStorageKeys)
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
        ArgumentException.ThrowIfNullOrWhiteSpace(reportStorageKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(reportContentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(reportFileName);

        return new RadiologyReport
        {
            LabOrderId = labOrderId,
            PatientId = patientId,
            HealthRecordId = healthRecordId,
            OrderingDoctorId = orderingDoctorId,
            LabPartnerCode = labPartnerCode.Trim().ToUpperInvariant(),
            LabPartnerOrderReference = labPartnerOrderReference.Trim(),
            ReportStorageKey = reportStorageKey.Trim(),
            ReportContentType = reportContentType.Trim(),
            ReportFileName = reportFileName.Trim(),
            ImagingStorageKeysJson = System.Text.Json.JsonSerializer.Serialize(imagingStorageKeys)
        };
    }
}
