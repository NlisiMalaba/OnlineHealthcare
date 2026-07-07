using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords.GrantHealthRecordAccess;

public sealed record GrantHealthRecordAccessCommand(
    Guid DoctorId,
    HealthRecordAccessType AccessType,
    IReadOnlyList<string>? Sections) : ICommand<HealthRecordAccessDto>;
