using HealthPlatform.Application.HealthRecords;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HealthPlatform.Infrastructure.HealthRecords;

public sealed class QuestPdfHealthRecordPdfGenerator : IHealthRecordPdfGenerator
{
    static QuestPdfHealthRecordPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(PatientHealthRecordExportModel model)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(style => style.FontSize(11));

                page.Header().Column(column =>
                {
                    column.Item().Text("Health Record Export").FontSize(18).SemiBold();
                    column.Item().Text($"Patient: {model.PatientDisplayName}");
                    column.Item().Text($"Health record ID: {model.HealthRecordId}");
                    column.Item().Text($"Generated: {model.GeneratedAtUtc:u}");
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    if (model.Entries.Count == 0)
                    {
                        column.Item().Text("No visible health record entries are available.");
                        return;
                    }

                    foreach (var entry in model.Entries.OrderByDescending(e => e.CreatedAtUtc))
                    {
                        column.Item().PaddingBottom(12).Column(entryColumn =>
                        {
                            entryColumn.Item()
                                .Text(HealthRecordEntryFormatter.FormatEntryType(entry.EntryType))
                                .FontSize(13)
                                .SemiBold();

                            foreach (var line in HealthRecordEntryFormatter.FormatEntryLines(entry))
                            {
                                entryColumn.Item().Text(line);
                            }
                        });

                        column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
