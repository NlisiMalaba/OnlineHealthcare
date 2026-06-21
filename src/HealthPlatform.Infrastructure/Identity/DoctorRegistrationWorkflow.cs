using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterDoctor;
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

public sealed class DoctorRegistrationWorkflow(
    UserManager<ApplicationUser> userManager,
    IDoctorRepository doctorRepository,
    IStorageService storageService,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    ILogger<DoctorRegistrationWorkflow> logger) : IDoctorRegistrationWorkflow
{
    public async Task<DoctorRegistrationResponseDto> RegisterAsync(
        RegisterDoctorCommand command,
        CancellationToken ct)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var normalizedPhone = command.PhoneNumber.Trim();
        var normalizedLicense = command.LicenseNumber.Trim().ToUpperInvariant();

        await EnsureNoIdentityConflictAsync(normalizedLicense, normalizedEmail, normalizedPhone, ct);

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

        await userManager.AddToRoleAsync(user, ApplicationRoles.Doctor);

        var doctorId = Guid.CreateVersion7();
        string? photoKey = null;
        if (command.ProfilePhoto is not null)
        {
            await using var photo = command.ProfilePhoto;
            var upload = await storageService.UploadDoctorProfilePhotoAsync(
                doctorId,
                photo.Content,
                photo.ContentType,
                photo.FileName,
                ct);
            photoKey = upload.StorageKey;
        }

        string credentialsKey;
        await using (var credentials = command.Credentials!)
        {
            var upload = await storageService.UploadDoctorCredentialsAsync(
                doctorId,
                credentials.Content,
                credentials.ContentType,
                credentials.FileName,
                ct);
            credentialsKey = upload.StorageKey;
        }

        GeoPoint? clinicLocation = null;
        if (command.ClinicLatitude.HasValue && command.ClinicLongitude.HasValue)
        {
            clinicLocation = new GeoPoint(command.ClinicLatitude.Value, command.ClinicLongitude.Value);
        }

        var availabilitySlots = command.AvailabilitySlots
            .Select(slot => DoctorAvailabilitySlot.Create(
                doctorId,
                slot.DayOfWeek,
                slot.StartTime,
                slot.EndTime,
                slot.SlotDurationMinutes,
                slot.AppointmentType))
            .ToList();

        var doctor = Doctor.Register(
            doctorId,
            user.Id,
            command.FullName,
            normalizedLicense,
            command.Specialty,
            command.YearsOfExperience,
            command.ClinicAddress,
            clinicLocation,
            command.VirtualFee,
            command.PhysicalFee,
            command.Bio,
            normalizedEmail,
            normalizedPhone,
            photoKey,
            credentialsKey,
            availabilitySlots);

        await doctorRepository.AddAsync(doctor, ct);

        var domainEvent = doctor.DomainEvents.OfType<DoctorRegisteredDomainEvent>().Single();
        await outboxRepository.EnqueueAsync(domainEvent, ct);
        await domainEventPublisher.PublishAsync(domainEvent, ct);
        doctor.ClearDomainEvents();

        logger.LogInformation(
            "Registered doctor {DoctorId} with user {UserId} in pending verification state.",
            doctor.Id,
            user.Id);

        return new DoctorRegistrationResponseDto(
            doctor.Id,
            doctor.VerificationStatus.ToString().ToLowerInvariant(),
            doctor.CreatedAtUtc);
    }

    private async Task EnsureNoIdentityConflictAsync(
        string licenseNumber,
        string email,
        string phoneNumber,
        CancellationToken ct)
    {
        if (await doctorRepository.ExistsByLicenseNumberAsync(licenseNumber, ct))
        {
            throw new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "A doctor account with this license number already exists.");
        }

        if (await doctorRepository.ExistsByEmailAsync(email, ct))
        {
            throw new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "A doctor account with this email already exists.");
        }

        if (await doctorRepository.ExistsByPhoneAsync(phoneNumber, ct))
        {
            throw new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "A doctor account with this phone number already exists.");
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            throw new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "A doctor account with this email already exists.");
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
                "A doctor account with this identifier already exists.");
        }

        var message = string.Join(" ", result.Errors.Select(e => e.Description));
        return new DomainException("REGISTRATION_FAILED", message);
    }
}
