namespace HealthPlatform.Application.Search;

public static class DoctorSearchProximityOrdering
{
    public static bool IsAscendingByDistance(IReadOnlyList<DoctorSearchMatchDto> results)
    {
        if (results.Count <= 1)
        {
            return true;
        }

        for (var index = 1; index < results.Count; index++)
        {
            var previous = results[index - 1].DistanceKilometers;
            var current = results[index].DistanceKilometers;

            if (!previous.HasValue || !current.HasValue)
            {
                return false;
            }

            if (current.Value + 1e-9 < previous.Value)
            {
                return false;
            }
        }

        return true;
    }
}
