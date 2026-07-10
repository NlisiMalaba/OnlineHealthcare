using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.BirthPlans;

public interface IMaternalCareAccessRepository
{
    Task AddAsync(MaternalCareAccessGrant grant, CancellationToken ct);

    Task UpdateAsync(MaternalCareAccessGrant grant, CancellationToken ct);

    Task<MaternalCareAccessGrant?> GetActiveGrantAsync(
        Guid antenatalRecordId,
        Guid doctorId,
        CancellationToken ct);

    Task<MaternalCareAccessGrant?> GetLatestGrantAsync(
        Guid antenatalRecordId,
        Guid doctorId,
        CancellationToken ct);

    Task<IReadOnlyList<MaternalCareAccessGrant>> ListActiveGrantsByAntenatalRecordIdAsync(
        Guid antenatalRecordId,
        CancellationToken ct);

    Task<IReadOnlyList<MaternalCareAccessGrant>> ListActiveGrantsByPatientIdAsync(
        Guid patientId,
        CancellationToken ct);
}
