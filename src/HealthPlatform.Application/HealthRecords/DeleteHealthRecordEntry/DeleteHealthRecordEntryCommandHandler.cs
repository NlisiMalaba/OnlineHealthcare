using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.HealthRecords.DeleteHealthRecordEntry;

public sealed class DeleteHealthRecordEntryCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    TimeProvider timeProvider)
    : IRequestHandler<DeleteHealthRecordEntryCommand>
{
    public async Task Handle(DeleteHealthRecordEntryCommand request, CancellationToken ct)
    {
        await ResolveVerifiedDoctorAsync(ct);

        var existing = await healthRecordEntryRepository.GetByIdAsync(request.EntryId, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordEntryNotFound,
                "Health record entry was not found.");

        if (existing.EntryType == HealthRecordEntryType.TelemedicineSessionSummary)
        {
            throw new DomainException(
                HealthRecordErrorCodes.InvalidEntryContent,
                "Telemedicine session summaries cannot be deleted.");
        }

        var deleted = await healthRecordEntryRepository.DeleteAsync(
            request.EntryId,
            timeProvider.GetUtcNow().UtcDateTime,
            ct);

        if (!deleted)
        {
            throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordEntryNotFound,
                "Health record entry was not found.");
        }
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
                "Only verified doctors can delete health record entries.");
        }

        return doctor;
    }
}
