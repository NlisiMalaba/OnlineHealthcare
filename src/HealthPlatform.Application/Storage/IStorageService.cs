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

    Task<string> GetSignedReadUrlAsync(string storageKey, CancellationToken ct);
}
