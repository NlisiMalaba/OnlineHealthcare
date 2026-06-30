using FluentValidation;
using MediatR;

namespace HealthPlatform.Application.Payments.GetPaymentReceipt;

public sealed record GetPaymentReceiptQuery(Guid PaymentId) : IRequest<PaymentReceiptDto>;

public sealed class GetPaymentReceiptQueryValidator : AbstractValidator<GetPaymentReceiptQuery>
{
    public GetPaymentReceiptQueryValidator()
    {
        RuleFor(q => q.PaymentId).NotEmpty();
    }
}
