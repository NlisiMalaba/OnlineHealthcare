using HealthPlatform.Application.Maternal.AntenatalRecords.ConfigureFetalMonitoringReminders;
using MediatR;

namespace HealthPlatform.Application.Maternal.AntenatalRecords.ConfigureFetalMonitoringReminders;

public sealed record ConfigureFetalMonitoringRemindersCommand(
    Guid AntenatalRecordId,
    int IntervalDays) : IRequest<AntenatalRecordDto>;
