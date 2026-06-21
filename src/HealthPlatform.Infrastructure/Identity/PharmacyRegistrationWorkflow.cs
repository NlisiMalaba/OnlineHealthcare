using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPharmacy;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Domain.ValueObjects;
using HealthPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Identity;

public sealed class PharmacyRegistrationWorkflow(
    UserManager<ApplicationUser> userManager,
    IPharmacyRepository pharmacyRepository,
    IStorageService storageService,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    ILogger<PharmacyRegistrationWorkflow> logger) : IPharmacyRegistrationWorkflow
{
    public async Task<PharmacyRegistrationResponseDto> RegisterAsync(
        RegisterPharmacyCommand command,
        CancellationToken ct)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var normalizedPhone = command.PhoneNumber.Trim();

        await EnsureNoIdentityConflictAsync(normalizedEmail, normalizedPhone, ct);

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            PhoneNumber = normalizedPhone
        };

        var createResult = await userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
        {
            throw MapIdentityFailure(createResult);
        }

        await userManager.AddToRoleAsync(user, ApplicationRoles.Pharmacy);

        var pharmacyId = Guid.CreateVersion7();
        string? logoKey = null;
        if (command.Logo is not null)
        {
            await using var logo = command.Logo;
            var upload = await storageService.UploadPharmacyLogoAsync(
                pharmacyId,
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

        var pharmacy = Pharmacy.Register(
            pharmacyId,
            user.Id,
            command.Name,
            command.Address,
            location,
            normalizedEmail,
            normalizedPhone,
            logoKey);

        await pharmacyRepository.AddAsync(pharmacy, ct);

        var domainEvent = pharmacy.DomainEvents.OfType<PharmacyRegisteredDomainEvent>().Single();
        await outboxRepository.EnqueueAsync(domainEvent, ct);
        await domainEventPublisher.PublishAsync(domainEvent, ct);

        logger.LogInformation(
            "Registered pharmacy {PharmacyId} with user {UserId} in pending verification state.",
            pharmacy.Id,
            user.Id);

        return new PharmacyRegistrationResponseDto(
            pharmacy.Id,
            pharmacy.VerificationStatus.ToString().ToLowerInvariant(),
            pharmacy.CreatedAtUtc);
    }

    private async Task EnsureNoIdentityConflictAsync(string email, string phoneNumber, CancellationToken ct)
    {
        if (await pharmacyRepository.ExistsByEmailAsync(email, ct))
        {
            throw new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "A pharmacy account with this email already exists.");
        }

        if (await pharmacyRepository.ExistsByPhoneAsync(phoneNumber, excludePharmacyId: null, ct))
        {
            throw new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "A pharmacy account with this phone number already exists.");
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            throw new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "A pharmacy account with this email already exists.");
        }
    }

    private static Exception MapIdentityFailure(IdentityResult result)
    {
        var duplicate = result.Errors.Any(e =>
            e.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)
            || e.Code.Contains("Already", StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            return new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "A pharmacy account with this identifier already exists.");
        }

        var message = string.Join(" ", result.Errors.Select(e => e.Description));
        return new DomainException("REGISTRATION_FAILED", message);
    }
}
