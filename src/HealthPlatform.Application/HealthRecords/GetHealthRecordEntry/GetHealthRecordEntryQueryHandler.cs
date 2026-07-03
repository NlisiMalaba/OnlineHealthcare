using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.HealthRecords.GetHealthRecordEntry;

public sealed class GetHealthRecordEntryQueryHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    IHealthRecordAccessGuard healthRecordAccessGuard)
    : IRequestHandler<GetHealthRecordEntryQuery, HealthRecordEntryDto>
{
    public async Task<HealthRecordEntryDto> Handle(GetHealthRecordEntryQuery request, CancellationToken ct)
    {
        var doctor = await ResolveVerifiedDoctorAsync(ct);

        var entry = await healthRecordEntryRepository.GetByIdAsync(request.EntryId, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordEntryNotFound,
                "Health record entry was not found.");

        await healthRecordAccessGuard.EnsureDoctorCanReadAsync(
            entry.HealthRecordId,
            doctor.Id,
            HealthRecordAccessOperations.GetEntry,
            ct);

        return entry;
    }

    private async Task<Doctor> ResolveVerifiedDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");
    }
}
