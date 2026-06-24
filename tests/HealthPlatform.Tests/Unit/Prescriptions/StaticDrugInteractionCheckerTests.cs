using HealthPlatform.Infrastructure.Prescriptions;
using Xunit;

namespace HealthPlatform.Tests.Unit.Prescriptions;

public sealed class StaticDrugInteractionCheckerTests
{
    private readonly StaticDrugInteractionChecker _checker = new();

    [Fact]
    public void Check_returns_alert_for_known_interacting_pair()
    {
        var alerts = _checker.Check("Ibuprofen", ["Warfarin"]);

        var alert = Assert.Single(alerts);
        Assert.Equal("Warfarin", alert.InteractingMedicationName);
        Assert.Contains("bleeding", alert.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Check_returns_empty_when_no_active_medications()
    {
        var alerts = _checker.Check("Ibuprofen", []);
        Assert.Empty(alerts);
    }
}
