using FluentValidation;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords.CreateHealthRecordEntry;

public sealed class CreateHealthRecordEntryCommandValidator : AbstractValidator<CreateHealthRecordEntryCommand>
{
    public CreateHealthRecordEntryCommandValidator()
    {
        RuleFor(command => command.HealthRecordId)
            .NotEmpty();

        RuleFor(command => command.EntryType)
            .IsInEnum()
            .Must(entryType => entryType != HealthRecordEntryType.TelemedicineSessionSummary)
            .WithMessage("Telemedicine session summaries are created automatically.")
            .Must(entryType => entryType != HealthRecordEntryType.TherapySessionSummary)
            .WithMessage("Therapy session summaries are created automatically.");

        RuleFor(command => command.Content)
            .Must((command, content) => HasMatchingContent(command.EntryType, content))
            .WithMessage("Content must match the selected entry type.");
    }

    private static bool HasMatchingContent(
        HealthRecordEntryType entryType,
        HealthRecordEntryContentPayload content) =>
        entryType switch
        {
            HealthRecordEntryType.ConsultationNote => content.ConsultationNote is not null,
            HealthRecordEntryType.Diagnosis => content.Diagnosis is not null,
            HealthRecordEntryType.PrescriptionRef => content.PrescriptionRef is not null,
            HealthRecordEntryType.Allergy => content.Allergy is not null,
            HealthRecordEntryType.Vital => content.Vital is not null,
            HealthRecordEntryType.LabResultRef => content.LabResultRef is not null,
            HealthRecordEntryType.Vaccination => content.Vaccination is not null,
            _ => false
        };
}
