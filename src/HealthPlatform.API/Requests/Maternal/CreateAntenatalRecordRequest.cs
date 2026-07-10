namespace HealthPlatform.API.Requests.Maternal;

public sealed record CreateAntenatalRecordRequest(
    DateOnly EstimatedDueDate,
    int GestationalAgeWeeks,
    Guid ObstetricDoctorId);
