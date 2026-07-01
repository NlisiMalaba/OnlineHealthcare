using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public sealed record NextOfKinContactDto(
    Guid Id,
    Guid PatientId,
    string FullName,
    string Relationship,
    string PhoneNumber,
    string? Email,
    bool IsMentalHealthContact);

public interface INextOfKinRepository
{
    Task<IReadOnlyList<NextOfKinContact>> ListByPatientIdAsync(Guid patientId, CancellationToken ct);

    Task<int> CountByPatientIdAsync(Guid patientId, CancellationToken ct);

    Task<NextOfKinContact?> GetByIdForPatientAsync(Guid contactId, Guid patientId, CancellationToken ct);

    Task AddAsync(NextOfKinContact contact, CancellationToken ct);

    Task UpdateAsync(NextOfKinContact contact, CancellationToken ct);

    Task DeleteAsync(NextOfKinContact contact, CancellationToken ct);
}
