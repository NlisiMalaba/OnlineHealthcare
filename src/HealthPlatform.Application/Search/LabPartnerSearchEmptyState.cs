namespace HealthPlatform.Application.Search;

internal static class LabPartnerSearchEmptyState
{
    public static string BuildMessage(Search.SearchLabPartners.SearchLabPartnersQuery query)
    {
        if (HasStrictFilters(query))
        {
            return "No lab partners match your current search filters.";
        }

        return "No lab partners are available in the search index yet.";
    }

    public static string BuildSuggestion(Search.SearchLabPartners.SearchLabPartnersQuery query)
    {
        var suggestions = new List<string>();

        if (!string.IsNullOrWhiteSpace(query.TestType))
        {
            suggestions.Add("try a different test type");
        }

        if (query.MinPrice.HasValue || query.MaxPrice.HasValue)
        {
            suggestions.Add("widen the price range");
        }

        if (query.PatientLatitude.HasValue && query.PatientLongitude.HasValue)
        {
            suggestions.Add("expand your search radius or disable location sorting");
        }

        if (suggestions.Count == 0)
        {
            return "Try again later or contact support if the problem persists.";
        }

        return $"Try broadening your search: {string.Join(", ", suggestions)}, or remove some filters.";
    }

    private static bool HasStrictFilters(Search.SearchLabPartners.SearchLabPartnersQuery query) =>
        !string.IsNullOrWhiteSpace(query.TestType)
        || query.MinPrice.HasValue
        || query.MaxPrice.HasValue
        || (query.PatientLatitude.HasValue && query.PatientLongitude.HasValue);
}
