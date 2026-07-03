using HealthPlatform.Application.Audit;
using HealthPlatform.Domain.Audit;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(ApplicationDbContext db) : IAuditLogRepository
{
    public async Task AppendAsync(AuditLog auditLog, CancellationToken ct)
    {
        await db.AuditLogs.AddAsync(auditLog, ct);
        await db.SaveChangesAsync(ct);
    }
}
