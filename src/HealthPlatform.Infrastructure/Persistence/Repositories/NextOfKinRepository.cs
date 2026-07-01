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

    public async Task AddAsync(NextOfKinContact contact, CancellationToken ct)
    {
        await db.NextOfKinContacts.AddAsync(contact, ct);
        await db.SaveChangesAsync(ct);
    }
}
