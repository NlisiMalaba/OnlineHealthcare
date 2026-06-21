using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity;

public interface IDoctorRepository
{
    Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, CancellationToken ct);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);

    Task<bool> ExistsByPhoneAsync(string phoneNumber, CancellationToken ct);

    Task AddAsync(Doctor doctor, CancellationToken ct);
}
