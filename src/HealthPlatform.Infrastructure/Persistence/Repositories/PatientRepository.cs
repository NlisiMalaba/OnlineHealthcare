using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class PatientRepository(ApplicationDbContext db) : IPatientRepository
{
    public Task<bool> ExistsByPhoneAsync(string phoneNumber, CancellationToken ct) =>
        db.Patients
            .AnyAsync(p => p.PhoneNumber == phoneNumber, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        db.Patients
            .AnyAsync(p => p.Email == email.ToLowerInvariant(), ct);

    public async Task AddAsync(Patient patient, CancellationToken ct)
    {
        await db.Patients.AddAsync(patient, ct);
        await db.SaveChangesAsync(ct);
    }
}
