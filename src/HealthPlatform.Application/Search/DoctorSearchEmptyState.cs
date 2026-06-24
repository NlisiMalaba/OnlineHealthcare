using HealthPlatform.Application.Search.SearchDoctors;

namespace HealthPlatform.Application.Search;

internal static class DoctorSearchEmptyState
{
    public static string BuildMessage(SearchDoctorsQuery query)
    {
        if (HasStrictFilters(query))
        {
            return "No doctors match your current search filters.";
        }

        return "No doctors are available in the search index yet.";
    }

    public static string BuildSuggestion(SearchDoctorsQuery query)
    {
        var suggestions = new List<string>();

        if (!string.IsNullOrWhiteSpace(query.Specialty))
        {
            suggestions.Add("try a different specialty");
        }

        if (query.MinRating.HasValue)
        {
            suggestions.Add("lower the minimum rating");
        }

        if (query.MinFee.HasValue || query.MaxFee.HasValue)
        {
            suggestions.Add("widen the consultation fee range");
        }

        if (query.HasAvailability == true)
        {
            suggestions.Add("include doctors without published availability");
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

    private static bool HasStrictFilters(SearchDoctorsQuery query) =>
        !string.IsNullOrWhiteSpace(query.Specialty)
        || query.MinRating.HasValue
        || query.MinFee.HasValue
        || query.MaxFee.HasValue
        || query.HasAvailability == true
        || (query.PatientLatitude.HasValue && query.PatientLongitude.HasValue);
}
