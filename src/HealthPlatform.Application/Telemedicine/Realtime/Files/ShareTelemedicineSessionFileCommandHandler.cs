using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Storage;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Telemedicine.Realtime.Files;

public sealed class ShareTelemedicineSessionFileCommandHandler(
    TimeProvider timeProvider,
    ITelemedicineSessionParticipantService participantService,
    IStorageService storageService,
    ITelemedicineRealtimeNotifier realtimeNotifier,
    ILogger<ShareTelemedicineSessionFileCommandHandler> logger)
    : IRequestHandler<ShareTelemedicineSessionFileCommand, TelemedicineFileSharedDto>
{
    public async Task<TelemedicineFileSharedDto> Handle(
        ShareTelemedicineSessionFileCommand request,
        CancellationToken ct)
    {
        var participant = await participantService.ResolveParticipantAsync(
            request.AppointmentId,
            requireActiveSession: true,
            ct);

        if (participant.Role != TelemedicineSessionParticipantRole.Doctor)
        {
            throw new AccessDeniedException(
                TelemedicineErrorCodes.FileShareNotAllowed,
                "Only the doctor can share files during a telemedicine session.");
        }

        var upload = await storageService.UploadTelemedicineSharedFileAsync(
            request.AppointmentId,
            request.Content,
            request.ContentType,
            request.FileName,
            ct);

        var downloadUrl = await storageService.GetSignedReadUrlAsync(upload.StorageKey, ct);
        var shared = new TelemedicineFileSharedDto(
            request.AppointmentId,
            Guid.CreateVersion7(),
            request.FileName,
            upload.ContentType,
            downloadUrl,
            timeProvider.GetUtcNow().UtcDateTime);

        await realtimeNotifier.PublishFileSharedAsync(shared, ct);

        logger.LogInformation(
            "Published telemedicine file share {ShareId} for appointment {AppointmentId}.",
            shared.ShareId,
            request.AppointmentId);

        return shared;
    }
}
