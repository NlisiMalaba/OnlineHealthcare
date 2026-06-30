using FsCheck.Xunit;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Payments.CreditLine;
using HealthPlatform.Application.Payments.CreditLine.EventHandlers;
using HealthPlatform.Application.Payments.CreditLine.Notifications;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Payments.CreditLine;
using HealthPlatform.Domain.Payments.CreditLine.Events;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Properties;

public sealed class CreditBalanceWarningThresholdPropertyTests
{
    // Feature: online-healthcare-platform, Property 15: Credit Balance Warning Threshold
    [Property(Arbitrary = [typeof(CreditBalanceWarningArbitraries)], MaxTest = 100)]
    public bool Balance_warning_is_emitted_iff_outstanding_exceeds_eighty_percent_after_charge(
        CreditBalanceWarningCase testCase)
    {
        var patientId = Guid.CreateVersion7();
        var newOutstanding = testCase.PreviousOutstandingMinorUnits + testCase.ChargeAmountMinorUnits;
        var shouldWarn = CreditLinePolicies.ShouldEmitBalanceWarning(
            newOutstanding,
            testCase.CreditLimitMinorUnits);

        var creditLine = PatientCreditLine.Open(
            patientId,
            testCase.CreditLimitMinorUnits,
            700m,
            "USD");

        if (testCase.PreviousOutstandingMinorUnits > 0)
        {
            creditLine.Charge(testCase.PreviousOutstandingMinorUnits, DateTime.UtcNow);
            creditLine.ClearDomainEvents();
        }

        creditLine.Charge(testCase.ChargeAmountMinorUnits, DateTime.UtcNow);

        var warningNotifier = new CapturingCreditBalanceWarningNotifier();
        var patient = Patient.RegisterWithEmail(
            Guid.CreateVersion7(),
            "Property Patient",
            $"property-{Guid.NewGuid():N}@example.com");
        var patientRepository = new Mock<IPatientRepository>();
        patientRepository
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var handler = new CreditBalanceWarningNotificationHandler(
            patientRepository.Object,
            warningNotifier);

        foreach (var domainEvent in creditLine.DomainEvents.OfType<CreditBalanceWarningTriggeredDomainEvent>())
        {
            handler.Handle(
                new CreditBalanceWarningNotification(
                    domainEvent.CreditLineId,
                    domainEvent.PatientId,
                    domainEvent.OutstandingBalanceMinorUnits,
                    domainEvent.CreditLimitMinorUnits,
                    domainEvent.Currency,
                    domainEvent.OccurredAtUtc),
                CancellationToken.None).GetAwaiter().GetResult();
        }

        if (shouldWarn)
        {
            return warningNotifier.Notifications.Count == 1
                && warningNotifier.Notifications[0].OutstandingBalanceMinorUnits == newOutstanding
                && warningNotifier.Notifications[0].CreditLimitMinorUnits == testCase.CreditLimitMinorUnits;
        }

        return warningNotifier.Notifications.Count == 0;
    }
}
