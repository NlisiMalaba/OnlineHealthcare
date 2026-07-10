using HealthPlatform.Domain.Queue;
using Xunit;

namespace HealthPlatform.Tests.Unit.Queue;

public sealed class QueueWaitTimeCalculatorTests
{
    [Fact]
    public void ComputeQueuePosition_returns_active_count_plus_one()
    {
        Assert.Equal(1, QueueWaitTimeCalculator.ComputeQueuePosition(0));
        Assert.Equal(3, QueueWaitTimeCalculator.ComputeQueuePosition(2));
    }

    [Fact]
    public void ComputeEstimatedWaitMinutes_multiplies_patients_ahead_by_duration()
    {
        Assert.Equal(0, QueueWaitTimeCalculator.ComputeEstimatedWaitMinutes(0, 30));
        Assert.Equal(60, QueueWaitTimeCalculator.ComputeEstimatedWaitMinutes(2, 30));
    }
}
