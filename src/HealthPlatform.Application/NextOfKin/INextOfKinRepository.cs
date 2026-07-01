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

    Task AddAsync(NextOfKinContact contact, CancellationToken ct);
}
