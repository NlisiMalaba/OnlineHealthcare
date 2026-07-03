using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.HealthRecords.ListHealthRecordEntries;

public sealed class ListHealthRecordEntriesQueryHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    IHealthRecordAccessGuard healthRecordAccessGuard)
    : IRequestHandler<ListHealthRecordEntriesQuery, IReadOnlyList<HealthRecordEntryDto>>
{
    public async Task<IReadOnlyList<HealthRecordEntryDto>> Handle(
        ListHealthRecordEntriesQuery request,
        CancellationToken ct)
    {
        var doctor = await ResolveVerifiedDoctorAsync(ct);

        _ = await healthRecordRepository.GetByIdAsync(request.HealthRecordId, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordNotFound,
                "Health record was not found.");

        await healthRecordAccessGuard.EnsureDoctorCanReadAsync(request.HealthRecordId, doctor.Id, ct);

        return await healthRecordEntryRepository.ListByHealthRecordIdAsync(
            request.HealthRecordId,
            patientVisibleOnly: false,
            ct);
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
