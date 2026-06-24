namespace HealthPlatform.Tests.Support;

public sealed class FakeTimeProvider(DateTime utcNow) : TimeProvider
{
    public DateTime UtcNow { get; private set; } = utcNow;

    public void SetUtcNow(DateTime value) => UtcNow = value;

    public override DateTimeOffset GetUtcNow() => new(UtcNow, TimeSpan.Zero);
}
