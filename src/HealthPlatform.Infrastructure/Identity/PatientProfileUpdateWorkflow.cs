using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Identity;

public sealed class PatientProfileUpdateWorkflow(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordProfileChangeRepository profileChangeRepository,
    IStorageService storageService,
    ILogger<PatientProfileUpdateWorkflow> logger) : IPatientProfileUpdateWorkflow
{
    public async Task<PatientProfileDto> UpdateAsync(UpdatePatientProfileCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var patient = await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");

        var healthRecord = await healthRecordRepository.GetByPatientIdAsync(patient.Id, ct)
            ?? throw new NotFoundException("HEALTH_RECORD_NOT_FOUND", "Linked health record was not found.");

        string? photoStorageKey = null;
        if (command.ProfilePhoto is not null)
        {
            await using var photo = command.ProfilePhoto;
            var upload = await storageService.UploadPatientProfilePhotoAsync(
                patient.Id,
                photo.Content,
                photo.ContentType,
                photo.FileName,
                ct);
            photoStorageKey = upload.StorageKey;
        }

        var changes = patient.UpdateProfile(
            command.FullName,
            command.DateOfBirth,
            command.BloodType,
            command.KnownAllergies,
            command.ChronicConditions,
            photoStorageKey);

        if (changes.Count == 0)
        {
            return await MapProfileAsync(patient, ct);
        }

        var auditEntries = changes
            .Select(change => HealthRecordProfileChange.Create(
                healthRecord.Id,
                patient.Id,
                change.FieldName,
                change.PreviousValue,
                change.NewValue,
                patient.UpdatedAtUtc))
            .ToList();

        await patientRepository.UpdateAsync(patient, ct);
        await profileChangeRepository.AddRangeAsync(auditEntries, ct);
        await patientRepository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Updated patient {PatientId} profile with {ChangeCount} health record changes.",
            patient.Id,
            auditEntries.Count);

        return await MapProfileAsync(patient, ct);
    }

    private async Task<PatientProfileDto> MapProfileAsync(Patient patient, CancellationToken ct)
    {
        string? photoUrl = null;
        if (!string.IsNullOrWhiteSpace(patient.ProfilePhotoStorageKey))
        {
            photoUrl = await storageService.GetSignedReadUrlAsync(patient.ProfilePhotoStorageKey, ct);
        }

        return new PatientProfileDto(
            patient.Id,
            patient.FullName,
            patient.DateOfBirth,
            patient.BloodType,
            patient.KnownAllergies,
            patient.ChronicConditions,
            photoUrl,
            patient.UpdatedAtUtc);
    }
}
