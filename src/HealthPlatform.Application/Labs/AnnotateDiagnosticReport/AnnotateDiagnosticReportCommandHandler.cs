using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Labs.AnnotateDiagnosticReport;

public sealed class AnnotateDiagnosticReportCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    ILabResultRepository labResultRepository,
    IRadiologyReportRepository radiologyReportRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    IHealthRecordAccessGuard healthRecordAccessGuard,
    TimeProvider timeProvider) : IRequestHandler<AnnotateDiagnosticReportCommand, HealthRecordEntryDto>
{
    public async Task<HealthRecordEntryDto> Handle(AnnotateDiagnosticReportCommand request, CancellationToken ct)
    {
        var doctor = await ResolveVerifiedDoctorAsync(ct);
        var target = await ResolveTargetAsync(request, ct);

        await healthRecordAccessGuard.EnsureDoctorCanReadAsync(
            target.HealthRecordId,
            doctor.Id,
            HealthRecordAccessOperations.AnnotateDiagnosticReport,
            ct);

        return await healthRecordEntryRepository.AddAsync(
            new HealthRecordEntryCreateModel(
                target.HealthRecordId,
                HealthRecordEntryType.DiagnosticReportAnnotation,
                new HealthRecordEntryContentPayload(
                    DiagnosticReportAnnotation: new DiagnosticReportAnnotationContent(
                        request.TargetType.ToString(),
                        request.TargetId,
                        request.Note.Trim(),
                        timeProvider.GetUtcNow().UtcDateTime)),
                doctor.Id,
                timeProvider.GetUtcNow().UtcDateTime,
                IsVisibleToPatient: true),
            ct);
    }

    private async Task<Doctor> ResolveVerifiedDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(LabOrderErrorCodes.DoctorNotFound, "Doctor profile was not found.");

        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            throw new DomainException(
                LabOrderErrorCodes.DoctorNotVerified,
                "Only verified doctors can annotate diagnostic reports.");
        }

        return doctor;
    }

    private async Task<AnnotationTarget> ResolveTargetAsync(AnnotateDiagnosticReportCommand request, CancellationToken ct)
    {
        return request.TargetType switch
        {
            DiagnosticAnnotationTargetType.LabResult => await ResolveLabResultTargetAsync(request.TargetId, ct),
            DiagnosticAnnotationTargetType.RadiologyReport => await ResolveRadiologyTargetAsync(request.TargetId, ct),
            _ => throw new DomainException("INVALID_TARGET_TYPE", "Unsupported annotation target type.")
        };
    }

    private async Task<AnnotationTarget> ResolveLabResultTargetAsync(Guid targetId, CancellationToken ct)
    {
        var result = await labResultRepository.GetByIdAsync(targetId, ct)
            ?? throw new NotFoundException(LabOrderErrorCodes.LabResultNotFound, "Lab result was not found.");
        return new AnnotationTarget(result.HealthRecordId);
    }

    private async Task<AnnotationTarget> ResolveRadiologyTargetAsync(Guid targetId, CancellationToken ct)
    {
        var report = await radiologyReportRepository.GetByIdAsync(targetId, ct)
            ?? throw new NotFoundException(
                LabOrderErrorCodes.RadiologyReportNotFound,
                "Radiology report was not found.");
        return new AnnotationTarget(report.HealthRecordId);
    }

    private sealed record AnnotationTarget(Guid HealthRecordId);
}
