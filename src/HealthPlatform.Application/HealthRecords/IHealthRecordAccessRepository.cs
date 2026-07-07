using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords;

public sealed record HealthRecordAccessDto(
    Guid Id,
    Guid HealthRecordId,
    Guid DoctorId,
    string DoctorFullName,
    HealthRecordAccessType AccessType,
    IReadOnlyList<string> Sections,
    DateTime GrantedAtUtc,
    DateTime? RevokedAtUtc,
    bool IsActive);

public interface IHealthRecordAccessRepository
{
    Task<HealthRecordAccess?> GetActiveGrantAsync(
        Guid healthRecordId,
        Guid doctorId,
        CancellationToken ct);

    Task<HealthRecordAccess?> GetLatestGrantAsync(
        Guid healthRecordId,
        Guid doctorId,
        CancellationToken ct);

    Task<IReadOnlyList<HealthRecordAccess>> ListByHealthRecordIdAsync(Guid healthRecordId, CancellationToken ct);

    Task AddAsync(HealthRecordAccess access, CancellationToken ct);

    Task UpdateAsync(HealthRecordAccess access, CancellationToken ct);
}
