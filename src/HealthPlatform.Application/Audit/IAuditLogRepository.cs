using HealthPlatform.Domain.Audit;

namespace HealthPlatform.Application.Audit;

public interface IAuditLogRepository
{
    Task AppendAsync(AuditLog auditLog, CancellationToken ct);
}
