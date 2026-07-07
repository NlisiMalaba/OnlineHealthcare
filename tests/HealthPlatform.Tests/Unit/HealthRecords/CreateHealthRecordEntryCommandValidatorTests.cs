using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.CreateHealthRecordEntry;
using HealthPlatform.Domain.HealthRecords;
using Xunit;

namespace HealthPlatform.Tests.Unit.HealthRecords;

public sealed class CreateHealthRecordEntryCommandValidatorTests
{
    private readonly CreateHealthRecordEntryCommandValidator _validator = new();

    [Fact]
    public void Validate_rejects_telemedicine_summary_entry_type()
    {
        var result = _validator.Validate(
            new CreateHealthRecordEntryCommand(
                Guid.CreateVersion7(),
                HealthRecordEntryType.TelemedicineSessionSummary,
                new HealthRecordEntryContentPayload(
                    TelemedicineSessionSummary: new TelemedicineSessionSummaryContent(
                        Guid.CreateVersion7(),
                        Guid.CreateVersion7(),
                        Guid.CreateVersion7(),
                        "summary-id")),
                IsVisibleToPatient: true));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_rejects_mismatched_content()
    {
        var result = _validator.Validate(
            new CreateHealthRecordEntryCommand(
                Guid.CreateVersion7(),
                HealthRecordEntryType.Diagnosis,
                new HealthRecordEntryContentPayload(
                    ConsultationNote: new ConsultationNoteContent("Notes", null)),
                IsVisibleToPatient: true));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_accepts_matching_diagnosis_content()
    {
        var result = _validator.Validate(
            new CreateHealthRecordEntryCommand(
                Guid.CreateVersion7(),
                HealthRecordEntryType.Diagnosis,
                new HealthRecordEntryContentPayload(
                    Diagnosis: new DiagnosisContent(["J06.9"], "Upper respiratory infection")),
                IsVisibleToPatient: true));

        Assert.True(result.IsValid);
    }
}
