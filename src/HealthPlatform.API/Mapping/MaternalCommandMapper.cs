using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;

namespace HealthPlatform.API.Mapping;

public static class MaternalCommandMapper
{
    public static CreateAntenatalRecordCommand ToCreateAntenatalRecordCommand(
        CreateAntenatalRecordRequest request) =>
        new(
            request.EstimatedDueDate,
            request.GestationalAgeWeeks,
            request.ObstetricDoctorId);
}
