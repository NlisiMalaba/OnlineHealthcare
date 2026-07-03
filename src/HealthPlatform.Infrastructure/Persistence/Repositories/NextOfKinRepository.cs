using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Domain.NextOfKin;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class NextOfKinRepository(ApplicationDbContext db) : INextOfKinRepository
{
    public async Task<IReadOnlyList<NextOfKinContact>> ListByPatientIdAsync(Guid patientId, CancellationToken ct) =>
        await db.NextOfKinContacts
            .AsNoTracking()
            .Where(contact => contact.PatientId == patientId)
            .OrderBy(contact => contact.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<int> CountByPatientIdAsync(Guid patientId, CancellationToken ct) =>
        await db.NextOfKinContacts
            .CountAsync(contact => contact.PatientId == patientId, ct);

    public async Task<NextOfKinContact?> GetByIdForPatientAsync(Guid contactId, Guid patientId, CancellationToken ct) =>
        await db.NextOfKinContacts
            .FirstOrDefaultAsync(contact => contact.Id == contactId && contact.PatientId == patientId, ct);

    public async Task AddAsync(NextOfKinContact contact, CancellationToken ct)
    {
        await db.NextOfKinContacts.AddAsync(contact, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(NextOfKinContact contact, CancellationToken ct)
    {
        db.NextOfKinContacts.Update(contact);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(NextOfKinContact contact, CancellationToken ct)
    {
        db.NextOfKinContacts.Remove(contact);
        await db.SaveChangesAsync(ct);
    }
}
