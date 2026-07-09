using HealthPlatform.Application.Security;

namespace HealthPlatform.Application.Notifications;

public static class NotificationPreferenceCatalog
{
  private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> EventTypesByRole =
      new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
      {
          [ApplicationRoles.Patient] =
          [
              NotificationEventTypes.AppointmentConfirmed,
              NotificationEventTypes.AppointmentReminder,
              NotificationEventTypes.AppointmentRescheduled,
              NotificationEventTypes.QueuePositionTwoReached,
              NotificationEventTypes.QueueMarkedAbsent,
              NotificationEventTypes.PrescriptionIssued,
              NotificationEventTypes.PrescriptionCancelled,
              NotificationEventTypes.MedicationDoseReminder,
              NotificationEventTypes.MedicationScheduleCompleted,
              NotificationEventTypes.OrderStatusChanged,
              NotificationEventTypes.PaymentFailed,
              NotificationEventTypes.CreditBalanceWarning,
              NotificationEventTypes.CreditRepaymentReminder,
              NotificationEventTypes.InstalmentDueReminder,
              NotificationEventTypes.InstalmentMissedPayment,
              NotificationEventTypes.AccountLocked
          ],
          [ApplicationRoles.Doctor] =
          [
              NotificationEventTypes.AppointmentConfirmed,
              NotificationEventTypes.AppointmentReminder,
              NotificationEventTypes.AppointmentRescheduled,
              NotificationEventTypes.DrugInteractionAlert,
              NotificationEventTypes.MedicationScheduleCompleted,
              NotificationEventTypes.DoctorLicenseVerified,
              NotificationEventTypes.DoctorLicenseRejected,
              NotificationEventTypes.AccountLocked
          ],
          [ApplicationRoles.Pharmacy] =
          [
              NotificationEventTypes.OrderReceived,
              NotificationEventTypes.LowStockAlert,
              NotificationEventTypes.AccountLocked
          ]
      };

    public static IReadOnlyList<string> GetConfigurableEventTypes(IReadOnlyList<string> roles)
    {
        var eventTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var role in roles)
        {
            if (EventTypesByRole.TryGetValue(role, out var roleEventTypes))
            {
                foreach (var eventType in roleEventTypes)
                {
                    eventTypes.Add(eventType);
                }
            }
        }

        return eventTypes.OrderBy(eventType => eventType, StringComparer.Ordinal).ToList();
    }

    public static bool IsConfigurableForRoles(string eventType, IReadOnlyList<string> roles) =>
        GetConfigurableEventTypes(roles).Contains(eventType, StringComparer.Ordinal);
}
