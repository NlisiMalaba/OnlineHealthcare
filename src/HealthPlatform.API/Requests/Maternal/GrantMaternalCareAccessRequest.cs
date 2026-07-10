namespace HealthPlatform.API.Requests.Maternal;

public sealed record GrantMaternalCareAccessRequest(
    Guid DoctorId,
    bool ShareAntenatalRecord,
    bool ShareBirthPlan);
