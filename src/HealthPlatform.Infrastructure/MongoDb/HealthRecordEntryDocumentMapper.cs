using System.Text.Json;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Infrastructure.MongoDb.Documents;
using MongoDB.Bson;

namespace HealthPlatform.Infrastructure.MongoDb;

internal static class HealthRecordEntryDocumentMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static HealthRecordEntryDocument ToDocument(HealthRecordEntryCreateModel request)
    {
        return new HealthRecordEntryDocument
        {
            HealthRecordId = request.HealthRecordId,
            EntryType = ToEntryTypeName(request.EntryType),
            Content = ToBsonDocument(request.Content),
            AuthoredBy = request.AuthoredBy,
            CreatedAtUtc = request.CreatedAtUtc,
            IsVisibleToPatient = request.IsVisibleToPatient
        };
    }

    public static HealthRecordEntryDto ToDto(HealthRecordEntryDocument document) =>
        new(
            document.Id.ToString(),
            document.HealthRecordId,
            ParseEntryType(document.EntryType),
            FromBsonDocument(document.Content, ParseEntryType(document.EntryType)),
            document.AuthoredBy,
            document.CreatedAtUtc,
            document.UpdatedAtUtc,
            document.IsVisibleToPatient);

    public static BsonDocument ToBsonDocument(HealthRecordEntryContentPayload content) =>
        BsonDocument.Parse(JsonSerializer.Serialize(content, SerializerOptions));

    public static HealthRecordEntryContentPayload FromBsonDocument(
        BsonDocument content,
        HealthRecordEntryType entryType)
    {
        var payload = JsonSerializer.Deserialize<HealthRecordEntryContentPayload>(
            content.ToJson(),
            SerializerOptions)
            ?? throw new InvalidOperationException("Health record entry content could not be deserialized.");

        return HealthRecordEntryContentResolver.Resolve(entryType, payload);
    }

    public static string ToEntryTypeName(HealthRecordEntryType entryType) =>
        entryType switch
        {
            HealthRecordEntryType.ConsultationNote => "consultation_note",
            HealthRecordEntryType.Diagnosis => "diagnosis",
            HealthRecordEntryType.PrescriptionRef => "prescription_ref",
            HealthRecordEntryType.Allergy => "allergy",
            HealthRecordEntryType.Vital => "vital",
            HealthRecordEntryType.LabResultRef => "lab_result_ref",
            HealthRecordEntryType.LabOrderRef => "lab_order_ref",
            HealthRecordEntryType.RadiologyReportRef => "radiology_report_ref",
            HealthRecordEntryType.DiagnosticReportAnnotation => "diagnostic_report_annotation",
            HealthRecordEntryType.Vaccination => "vaccination",
            HealthRecordEntryType.TelemedicineSessionSummary => "telemedicine_session_summary",
            _ => throw new ArgumentOutOfRangeException(nameof(entryType), entryType, "Unsupported entry type.")
        };

    public static HealthRecordEntryType ParseEntryType(string entryType) =>
        entryType switch
        {
            "consultation_note" => HealthRecordEntryType.ConsultationNote,
            "diagnosis" => HealthRecordEntryType.Diagnosis,
            "prescription_ref" => HealthRecordEntryType.PrescriptionRef,
            "allergy" => HealthRecordEntryType.Allergy,
            "vital" => HealthRecordEntryType.Vital,
            "lab_result_ref" => HealthRecordEntryType.LabResultRef,
            "lab_order_ref" => HealthRecordEntryType.LabOrderRef,
            "radiology_report_ref" => HealthRecordEntryType.RadiologyReportRef,
            "diagnostic_report_annotation" => HealthRecordEntryType.DiagnosticReportAnnotation,
            "vaccination" => HealthRecordEntryType.Vaccination,
            "telemedicine_session_summary" => HealthRecordEntryType.TelemedicineSessionSummary,
            _ => throw new ArgumentOutOfRangeException(nameof(entryType), entryType, "Unsupported entry type.")
        };
}
