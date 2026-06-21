using HealthPlatform.Application.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string BucketName { get; set; } = "healthplatform-patient-assets";

    public string LocalRootPath { get; set; } = "uploads";

    public int SignedUrlTtlMinutes { get; set; } = 60;
}

public sealed class LocalFileStorageService(
    IOptions<StorageOptions> options,
    ILogger<LocalFileStorageService> logger) : IStorageService
{
    private readonly StorageOptions _options = options.Value;

    public async Task<StorageUploadResult> UploadPatientProfilePhotoAsync(
        Guid patientId,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken ct)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = contentType switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
        }

        var storageKey = $"patients/{patientId}/profile-photo/{Guid.CreateVersion7():N}{extension}";
        return await StoreAsync(storageKey, content, contentType, ct);
    }

    public async Task<StorageUploadResult> UploadDoctorProfilePhotoAsync(
        Guid doctorId,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken ct)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = contentType switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
        }

        var storageKey = $"doctors/{doctorId}/profile-photo/{Guid.CreateVersion7():N}{extension}";
        return await StoreAsync(storageKey, content, contentType, ct);
    }

    public async Task<StorageUploadResult> UploadDoctorCredentialsAsync(
        Guid doctorId,
        Stream content,
        string contentType,
        string fileName,
        CancellationToken ct)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = contentType switch
            {
                "application/pdf" => ".pdf",
                "image/png" => ".png",
                _ => ".jpg"
            };
        }

        var storageKey = $"doctors/{doctorId}/credentials/{Guid.CreateVersion7():N}{extension}";
        return await StoreAsync(storageKey, content, contentType, ct);
    }

    public Task<string> GetSignedReadUrlAsync(string storageKey, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var absolutePath = GetAbsolutePath(storageKey);
        return Task.FromResult($"file:///{absolutePath.Replace('\\', '/')}");
    }

    private async Task<StorageUploadResult> StoreAsync(
        string storageKey,
        Stream content,
        string contentType,
        CancellationToken ct)
    {
        var absolutePath = GetAbsolutePath(storageKey);
        var directory = Path.GetDirectoryName(absolutePath)
            ?? throw new InvalidOperationException("Invalid storage path.");

        Directory.CreateDirectory(directory);

        await using var fileStream = File.Create(absolutePath);
        await content.CopyToAsync(fileStream, ct);

        logger.LogInformation("Stored file at key {StorageKey}.", storageKey);
        return new StorageUploadResult(storageKey, contentType);
    }

    private string GetAbsolutePath(string storageKey)
    {
        var root = Path.GetFullPath(_options.LocalRootPath);
        var combined = Path.GetFullPath(Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar)));

        if (!combined.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Storage key resolves outside the configured root.");
        }

        return combined;
    }
}
