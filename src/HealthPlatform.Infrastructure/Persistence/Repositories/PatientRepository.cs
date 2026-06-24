using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class PatientRepository(ApplicationDbContext db) : IPatientRepository
{
    public Task<bool> ExistsByPhoneAsync(string phoneNumber, CancellationToken ct) =>
        db.Patients.AnyAsync(p => p.PhoneNumber == phoneNumber, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        db.Patients.AnyAsync(p => p.Email == email.ToLowerInvariant(), ct);

    public Task<Patient?> GetByUserIdAsync(Guid userId, CancellationToken ct) =>
        db.Patients.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public Task<Patient?> GetByIdAsync(Guid patientId, CancellationToken ct) =>
        db.Patients.FirstOrDefaultAsync(p => p.Id == patientId, ct);

    public async Task AddAsync(Patient patient, CancellationToken ct)
    {
        await db.Patients.AddAsync(patient, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(Patient patient, CancellationToken ct)
    {
        db.Patients.Update(patient);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
