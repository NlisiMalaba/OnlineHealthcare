using HealthPlatform.Domain.Payments.CreditLine;

namespace HealthPlatform.Application.Payments.CreditLine;

public static class CreditLineMappings
{
    public static CreditLineDto ToDto(this PatientCreditLine creditLine) =>
        new(
            creditLine.Id,
            creditLine.CreditLimitMinorUnits,
            creditLine.OutstandingBalanceMinorUnits,
            creditLine.CreditLimitMinorUnits - creditLine.OutstandingBalanceMinorUnits,
            creditLine.CreditScore,
            creditLine.Currency);
}
