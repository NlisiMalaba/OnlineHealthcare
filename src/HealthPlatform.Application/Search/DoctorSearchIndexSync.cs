using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Search;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Search;

internal static class DoctorSearchIndexSync
{
    public static async Task UpsertVerifiedDoctorAsync(
        IDoctorRepository doctorRepository,
        ISearchService searchService,
        Guid doctorId,
        CancellationToken ct)
    {
        var doctor = await doctorRepository.GetByIdAsync(doctorId, ct);
        if (doctor is null || doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            return;
        }

        await searchService.UpsertDoctorAsync(doctorId, ct);
    }
}
