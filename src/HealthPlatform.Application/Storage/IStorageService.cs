namespace HealthPlatform.Application.Storage;

public sealed record StorageUploadResult(string StorageKey, string ContentType);

public interface IStorageService
{
    Task<StorageUploadResult> UploadPatientProfilePhotoAsync(
        Guid patientId,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken ct);

    Task<StorageUploadResult> UploadDoctorProfilePhotoAsync(
        Guid doctorId,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken ct);

    Task<StorageUploadResult> UploadDoctorCredentialsAsync(
        Guid doctorId,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken ct);

    Task<StorageUploadResult> UploadPharmacyLogoAsync(
        Guid pharmacyId,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken ct);

    Task<string> GetSignedReadUrlAsync(string storageKey, CancellationToken ct);

    Task<StorageUploadResult> UploadTelemedicineSharedFileAsync(
        Guid appointmentId,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken ct);

    Task<StorageUploadResult> UploadPaymentReceiptAsync(
        Guid patientId,
        Guid paymentId,
        Stream content,
        CancellationToken ct);

    Task<StorageUploadResult> UploadHealthRecordExportAsync(
        Guid patientId,
        Guid healthRecordId,
        Stream content,
        CancellationToken ct);

    Task<StorageUploadResult> UploadLabResultAsync(
        Guid patientId,
        Guid labOrderId,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken ct);

    Task<StorageUploadResult> UploadRadiologyReportAsync(
        Guid patientId,
        Guid labOrderId,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken ct);

    Task<StorageUploadResult> UploadRadiologyImagingFileAsync(
        Guid patientId,
        Guid labOrderId,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken ct);
}
