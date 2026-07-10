using HealthPlatform.Application.Maternal.AntenatalRecords;
using MediatR;

namespace HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;

public sealed record CreateAntenatalRecordCommand(
    DateOnly EstimatedDueDate,
    int GestationalAgeWeeks,
    Guid ObstetricDoctorId) : IRequest<AntenatalRecordDto>;
