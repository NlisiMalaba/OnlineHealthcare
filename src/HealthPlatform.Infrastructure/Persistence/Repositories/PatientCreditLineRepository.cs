using HealthPlatform.Application.Payments.CreditLine;
using HealthPlatform.Domain.Payments.CreditLine;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class PatientCreditLineRepository(ApplicationDbContext db) : IPatientCreditLineRepository
{
    public Task<PatientCreditLine?> GetByPatientIdAsync(Guid patientId, CancellationToken ct) =>
        db.PatientCreditLines.FirstOrDefaultAsync(c => c.PatientId == patientId, ct);

    public async Task AddAsync(PatientCreditLine creditLine, CancellationToken ct)
    {
        await db.PatientCreditLines.AddAsync(creditLine, ct);
    }

    public Task UpdateAsync(PatientCreditLine creditLine, CancellationToken ct)
    {
        db.PatientCreditLines.Update(creditLine);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
