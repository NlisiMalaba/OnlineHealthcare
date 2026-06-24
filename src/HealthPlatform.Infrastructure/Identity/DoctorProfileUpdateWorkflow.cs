using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.UpdateDoctorProfile;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.Identity;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Identity;

public sealed class DoctorProfileUpdateWorkflow(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IStorageService storageService,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    ILogger<DoctorProfileUpdateWorkflow> logger) : IDoctorProfileUpdateWorkflow
{
    public async Task<DoctorProfileDto> UpdateAsync(UpdateDoctorProfileCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                IdentityErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        string? photoKey = null;
        if (command.ProfilePhoto is not null)
        {
            await using var photo = command.ProfilePhoto;
            var upload = await storageService.UploadDoctorProfilePhotoAsync(
                doctor.Id,
                photo.Content,
                photo.ContentType,
                photo.FileName,
                ct);
            photoKey = upload.StorageKey;
        }

        string? credentialsKey = null;
        if (command.Credentials is not null)
        {
            await using var credentials = command.Credentials;
            var upload = await storageService.UploadDoctorCredentialsAsync(
                doctor.Id,
                credentials.Content,
                credentials.ContentType,
                credentials.FileName,
                ct);
            credentialsKey = upload.StorageKey;
        }

        doctor.UpdateProfile(
            command.VirtualFee,
            command.PhysicalFee,
            command.Bio,
            photoKey,
            credentialsKey);

        if (command.AvailabilitySlots is not null)
        {
            var replacementSlots = command.AvailabilitySlots
                .Select(slot => DoctorAvailabilitySlot.Create(
                    doctor.Id,
                    slot.DayOfWeek,
                    slot.StartTime,
                    slot.EndTime,
                    slot.SlotDurationMinutes,
                    slot.AppointmentType))
                .ToList();

            if (doctor.ApplyAvailabilityReplacement(replacementSlots))
            {
                await doctorRepository.ReplaceAvailabilitySlotsAsync(doctor.Id, replacementSlots, ct);
                doctor.SetAvailabilitySlots(replacementSlots);
            }
        }

        var pendingEvents = doctor.DomainEvents.ToList();
        await doctorRepository.UpdateAsync(doctor, ct);

        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        doctor.ClearDomainEvents();

        logger.LogInformation(
            "Updated doctor {DoctorId} profile with {DomainEventCount} domain events.",
            doctor.Id,
            pendingEvents.Count);

        return await MapProfileAsync(doctor, ct);
    }

    private async Task<DoctorProfileDto> MapProfileAsync(Doctor doctor, CancellationToken ct)
    {
        string? photoUrl = null;
        if (!string.IsNullOrWhiteSpace(doctor.ProfilePhotoStorageKey))
        {
            photoUrl = await storageService.GetSignedReadUrlAsync(doctor.ProfilePhotoStorageKey, ct);
        }

        var slots = doctor.AvailabilitySlots
            .Select(slot => new DoctorAvailabilitySlotDto(
                slot.Id,
                slot.DayOfWeek,
                slot.StartTime,
                slot.EndTime,
                slot.SlotDurationMinutes,
                slot.AppointmentType,
                slot.IsActive))
            .ToList();

        return new DoctorProfileDto(
            doctor.Id,
            doctor.FullName,
            doctor.Specialty,
            doctor.VirtualFee,
            doctor.PhysicalFee,
            doctor.Bio,
            photoUrl,
            doctor.VerificationStatus.ToString().ToLowerInvariant(),
            slots,
            doctor.UpdatedAtUtc);
    }
}
