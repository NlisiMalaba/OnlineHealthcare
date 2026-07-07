using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.HealthRecords.RevokeHealthRecordAccess;

public sealed record RevokeHealthRecordAccessCommand(Guid DoctorId) : ICommand<HealthRecordAccessDto>;
