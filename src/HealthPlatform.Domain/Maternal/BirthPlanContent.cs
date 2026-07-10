namespace HealthPlatform.Domain.Maternal;

public sealed record BirthPlanContent(
    string? LabourPreferences,
    string? DeliveryMethod,
    string? PainManagement,
    string? PostnatalCare)
{
    public bool IsEmpty() =>
        string.IsNullOrWhiteSpace(LabourPreferences)
        && string.IsNullOrWhiteSpace(DeliveryMethod)
        && string.IsNullOrWhiteSpace(PainManagement)
        && string.IsNullOrWhiteSpace(PostnatalCare);
}
