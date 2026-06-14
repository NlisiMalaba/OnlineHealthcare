using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity;

public interface IPatientRepository
{
    Task<bool> ExistsByPhoneAsync(string phoneNumber, CancellationToken ct);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);

    Task AddAsync(Patient patient, CancellationToken ct);
}
