using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Payments.CreditLine.Events;

namespace HealthPlatform.Domain.Payments.CreditLine;

public sealed class PatientCreditLine : Entity
{
    private PatientCreditLine()
    {
        Currency = string.Empty;
    }

    public Guid PatientId { get; private set; }

    public long CreditLimitMinorUnits { get; private set; }

    public long OutstandingBalanceMinorUnits { get; private set; }

    public decimal CreditScore { get; private set; }

    public string Currency { get; private set; }

    public static PatientCreditLine Open(
        Guid patientId,
        long creditLimitMinorUnits,
        decimal creditScore,
        string currency)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (creditLimitMinorUnits <= 0)
        {
            throw new ArgumentException("Credit limit must be positive.", nameof(creditLimitMinorUnits));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new PatientCreditLine
        {
            PatientId = patientId,
            CreditLimitMinorUnits = creditLimitMinorUnits,
            OutstandingBalanceMinorUnits = 0,
            CreditScore = creditScore,
            Currency = currency.Trim().ToUpperInvariant()
        };
    }

    public CreditLineChargeResult Charge(long amountMinorUnits, DateTime chargedAtUtc)
    {
        if (amountMinorUnits <= 0)
        {
            throw new ArgumentException("Charge amount must be positive.", nameof(amountMinorUnits));
        }

        if (!CreditLinePolicies.HasAvailableCredit(
                OutstandingBalanceMinorUnits,
                CreditLimitMinorUnits,
                amountMinorUnits))
        {
            throw new CreditLimitExceededException();
        }

        var previousBalance = OutstandingBalanceMinorUnits;
        OutstandingBalanceMinorUnits += amountMinorUnits;
        Touch();

        var balanceWarningRequired = CreditLinePolicies.ShouldEmitBalanceWarning(
            OutstandingBalanceMinorUnits,
            CreditLimitMinorUnits);

        RaiseDomainEvent(new CreditLineChargedDomainEvent(
            Id,
            PatientId,
            amountMinorUnits,
            Currency,
            OutstandingBalanceMinorUnits,
            CreditLimitMinorUnits,
            chargedAtUtc));

        if (balanceWarningRequired)
        {
            RaiseDomainEvent(new CreditBalanceWarningTriggeredDomainEvent(
                Id,
                PatientId,
                OutstandingBalanceMinorUnits,
                CreditLimitMinorUnits,
                Currency));
        }

        return new CreditLineChargeResult(
            previousBalance,
            OutstandingBalanceMinorUnits,
            balanceWarningRequired);
    }
}

public sealed record CreditLineChargeResult(
    long PreviousOutstandingBalanceMinorUnits,
    long NewOutstandingBalanceMinorUnits,
    bool BalanceWarningRequired);
