using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Infrastructure.HealthRecords;
using Xunit;

namespace HealthPlatform.Tests.Unit.HealthRecords;

public sealed class HealthRecordPdfGeneratorTests
{
    [Fact]
    public void Generate_produces_valid_pdf_document()
    {
        var generator = new QuestPdfHealthRecordPdfGenerator();
        var healthRecordId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();

        var pdfBytes = generator.Generate(
            new PatientHealthRecordExportModel(
                healthRecordId,
                patientId,
                "Test Patient",
                new DateTime(2026, 7, 3, 10, 0, 0, DateTimeKind.Utc),
                [
                    new HealthRecordEntryDto(
                        "entry-1",
                        healthRecordId,
                        HealthRecordEntryType.ConsultationNote,
                        new HealthRecordEntryContentPayload(
                            ConsultationNote: new ConsultationNoteContent("Routine follow-up.", null)),
                        Guid.CreateVersion7(),
                        new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc),
                        null,
                        true)
                ]));

        Assert.NotEmpty(pdfBytes);
        Assert.Equal(0x25, pdfBytes[0]);
        Assert.Equal((byte)'P', pdfBytes[1]);
        Assert.Equal((byte)'D', pdfBytes[2]);
        Assert.Equal((byte)'F', pdfBytes[3]);
    }
}
