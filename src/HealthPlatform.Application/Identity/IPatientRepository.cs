using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity;

public interface IPatientRepository
{
    Task<bool> ExistsByPhoneAsync(string phoneNumber, CancellationToken ct);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);

    Task<Patient?> GetByUserIdAsync(Guid userId, CancellationToken ct);

    Task<Patient?> GetByIdAsync(Guid patientId, CancellationToken ct);

    Task AddAsync(Patient patient, CancellationToken ct);

    Task UpdateAsync(Patient patient, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
