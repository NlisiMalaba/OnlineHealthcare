using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.HealthRecords.CreateHealthRecordEntry;

public sealed class CreateHealthRecordEntryCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    TimeProvider timeProvider)
    : IRequestHandler<CreateHealthRecordEntryCommand, HealthRecordEntryDto>
{
    public async Task<HealthRecordEntryDto> Handle(CreateHealthRecordEntryCommand request, CancellationToken ct)
    {
        var doctor = await ResolveVerifiedDoctorAsync(ct);
        var healthRecord = await healthRecordRepository.GetByIdAsync(request.HealthRecordId, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordNotFound,
                "Health record was not found.");

        var content = HealthRecordEntryContentResolver.Resolve(request.EntryType, request.Content);
        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        return await healthRecordEntryRepository.AddAsync(
            new HealthRecordEntryCreateModel(
                healthRecord.Id,
                request.EntryType,
                content,
                doctor.Id,
                createdAtUtc,
                request.IsVisibleToPatient),
            ct);
    }

    private async Task<Doctor> ResolveVerifiedDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            throw new DomainException(
                HealthRecordErrorCodes.DoctorNotVerified,
                "Only verified doctors can add health record entries.");
        }

        return doctor;
    }
}
