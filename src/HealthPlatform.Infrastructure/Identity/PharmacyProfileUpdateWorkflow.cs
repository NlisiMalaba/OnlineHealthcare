using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.UpdatePharmacyProfile;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Identity;

public sealed class PharmacyProfileUpdateWorkflow(
    ICurrentUserAccessor currentUser,
    IPharmacyRepository pharmacyRepository,
    IStorageService storageService,
    ILogger<PharmacyProfileUpdateWorkflow> logger) : IPharmacyProfileUpdateWorkflow
{
    public async Task<PharmacyProfileDto> UpdateAsync(UpdatePharmacyProfileCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var pharmacy = await pharmacyRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                IdentityErrorCodes.PharmacyNotFound,
                "Pharmacy profile was not found.");

        if (command.PhoneNumber is not null)
        {
            var normalizedPhone = command.PhoneNumber.Trim();
            if (await pharmacyRepository.ExistsByPhoneAsync(normalizedPhone, pharmacy.Id, ct))
            {
                throw new ConflictException(
                    IdentityErrorCodes.IdentityConflict,
                    "A pharmacy account with this phone number already exists.");
            }
        }

        string? logoKey = null;
        if (command.Logo is not null)
        {
            await using var logo = command.Logo;
            var upload = await storageService.UploadPharmacyLogoAsync(
                pharmacy.Id,
                logo.Content,
                logo.ContentType,
                logo.FileName,
                ct);
            logoKey = upload.StorageKey;
        }

        GeoPoint? location = null;
        if (command.Latitude.HasValue && command.Longitude.HasValue)
        {
            location = new GeoPoint(command.Latitude.Value, command.Longitude.Value);
        }

        pharmacy.UpdateProfile(
            command.Name,
            command.Address,
            location,
            command.PhoneNumber?.Trim(),
            logoKey);

        await pharmacyRepository.UpdateAsync(pharmacy, ct);

        logger.LogInformation("Updated pharmacy {PharmacyId} profile.", pharmacy.Id);

        return await MapProfileAsync(pharmacy, ct);
    }

    private async Task<PharmacyProfileDto> MapProfileAsync(Pharmacy pharmacy, CancellationToken ct)
    {
        string? logoUrl = null;
        if (!string.IsNullOrWhiteSpace(pharmacy.LogoStorageKey))
        {
            logoUrl = await storageService.GetSignedReadUrlAsync(pharmacy.LogoStorageKey, ct);
        }

        return new PharmacyProfileDto(
            pharmacy.Id,
            pharmacy.Name,
            pharmacy.Address,
            pharmacy.Location?.Latitude,
            pharmacy.Location?.Longitude,
            pharmacy.ContactEmail,
            pharmacy.ContactPhone,
            logoUrl,
            pharmacy.VerificationStatus.ToString().ToLowerInvariant(),
            pharmacy.UpdatedAtUtc);
    }
}
