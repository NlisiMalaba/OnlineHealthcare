using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.API.Requests.HealthRecords;

public sealed class GrantHealthRecordAccessRequest
{
    public required Guid DoctorId { get; init; }

    public HealthRecordAccessType AccessType { get; init; } = HealthRecordAccessType.Full;

    public IReadOnlyList<string>? Sections { get; init; }
}
