using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Referrals;
using MediatR;

namespace HealthPlatform.Application.Referrals.RespondToReferral;

public sealed class RespondToReferralCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IReferralRepository referralRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<RespondToReferralCommand, ReferralDto>
{
    public async Task<ReferralDto> Handle(RespondToReferralCommand request, CancellationToken ct)
    {
        var receivingDoctor = await ResolveDoctorAsync(ct);
        var referral = await referralRepository.GetByIdAsync(request.ReferralId, ct)
            ?? throw new NotFoundException(
                ReferralErrorCodes.ReferralNotFound,
                "Referral was not found.");

        if (!referral.ReceivingDoctorId.HasValue || referral.ReceivingDoctorId.Value != receivingDoctor.Id)
        {
            throw new AccessDeniedException(
                ReferralErrorCodes.ReferralAccessDenied,
                "Only the receiving doctor can respond to this referral.");
        }

        var respondedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        try
        {
            switch (request.Action)
            {
                case ReferralResponseAction.Accept:
                    referral.Accept(respondedAtUtc);
                    var accessGrant = ReferralHealthRecordAccessGrant.Create(
                        referral.Id,
                        referral.PatientId,
                        receivingDoctor.Id,
                        referral.SharedHealthRecordSections,
                        respondedAtUtc);
                    await referralRepository.AddAccessGrantAsync(accessGrant, ct);
                    break;
                case ReferralResponseAction.Decline:
                    referral.Decline(request.Reason!, respondedAtUtc);
                    break;
                case ReferralResponseAction.RequestAdditionalInformation:
                    referral.RequestAdditionalInformation(request.Reason!, respondedAtUtc);
                    break;
                default:
                    throw new DomainException(
                        ReferralErrorCodes.ReferralResponseNotAllowed,
                        "Unsupported referral response action.");
            }
        }
        catch (ReferralResponseNotAllowedException ex)
        {
            throw new DomainException(
                ReferralErrorCodes.ReferralResponseNotAllowed,
                ex.Message);
        }

        await referralRepository.UpdateAsync(referral, ct);
        foreach (var domainEvent in referral.DomainEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }
        referral.ClearDomainEvents();

        return referral.ToDto();
    }

    private async Task<Domain.Identity.Doctor> ResolveDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                ReferralErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");
    }
}
