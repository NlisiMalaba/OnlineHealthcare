using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Queue;

public static class QueueConsultationDurationResolver
{
    public static int ResolveAverageMinutes(IReadOnlyCollection<DoctorAvailabilitySlot> slots)
    {
        var activeDurations = slots
            .Where(slot => slot.IsActive)
            .Select(slot => slot.SlotDurationMinutes)
            .ToList();

        if (activeDurations.Count == 0)
        {
            return QueuePolicies.DefaultConsultationDurationMinutes;
        }

        return (int)Math.Round(activeDurations.Average());
    }
}
