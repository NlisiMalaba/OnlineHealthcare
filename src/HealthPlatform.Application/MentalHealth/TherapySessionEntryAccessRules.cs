using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.MentalHealth;

public static class TherapySessionEntryAccessRules
{
    public static bool CanDoctorView(
        HealthRecordEntryDto entry,
        Guid doctorId) =>
        entry.EntryType != HealthRecordEntryType.TherapySessionSummary
        || entry.Content.TherapySessionSummary is not { } summary
        || summary.TherapistId == doctorId
        || summary.BroaderAccessGranted;

    public static IReadOnlyList<HealthRecordEntryDto> FilterForDoctor(
        IReadOnlyList<HealthRecordEntryDto> entries,
        Guid doctorId) =>
        entries.Where(entry => CanDoctorView(entry, doctorId)).ToList();
}
