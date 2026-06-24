namespace HealthPlatform.Application.Search;

internal static class PharmacySearchEmptyState
{
    public static string BuildMessage(Search.SearchPharmacies.SearchPharmaciesQuery query)
    {
        if (HasStrictFilters(query))
        {
            return "No pharmacies match your current search filters.";
        }

        return "No pharmacies are available in the search index yet.";
    }

    public static string BuildSuggestion(Search.SearchPharmacies.SearchPharmaciesQuery query)
    {
        var suggestions = new List<string>();

        if (!string.IsNullOrWhiteSpace(query.MedicationSku))
        {
            suggestions.Add("search for a different medication SKU");
        }

        if (query.HasStock == true)
        {
            suggestions.Add("include pharmacies without reported stock");
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

    private static bool HasStrictFilters(Search.SearchPharmacies.SearchPharmaciesQuery query) =>
        !string.IsNullOrWhiteSpace(query.MedicationSku)
        || query.HasStock == true
        || (query.PatientLatitude.HasValue && query.PatientLongitude.HasValue);
}
