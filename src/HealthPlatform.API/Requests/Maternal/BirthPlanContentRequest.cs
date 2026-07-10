namespace HealthPlatform.API.Requests.Maternal;

public sealed record BirthPlanContentRequest(
    string? LabourPreferences,
    string? DeliveryMethod,
    string? PainManagement,
    string? PostnatalCare);
