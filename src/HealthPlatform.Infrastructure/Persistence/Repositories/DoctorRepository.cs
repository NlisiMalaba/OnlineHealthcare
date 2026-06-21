using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class DoctorRepository(ApplicationDbContext db) : IDoctorRepository
{
    public Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, CancellationToken ct) =>
        db.Doctors.AnyAsync(
            d => d.LicenseNumber == licenseNumber.Trim().ToUpperInvariant(),
            ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        db.Doctors.AnyAsync(d => d.Email == email.Trim().ToLowerInvariant(), ct);

    public Task<bool> ExistsByPhoneAsync(string phoneNumber, CancellationToken ct) =>
        db.Doctors.AnyAsync(d => d.PhoneNumber == phoneNumber.Trim(), ct);

    public async Task AddAsync(Doctor doctor, CancellationToken ct)
    {
        await db.Doctors.AddAsync(doctor, ct);
        await db.SaveChangesAsync(ct);
    }
}
