namespace HealthPlatform.Domain.ValueObjects;

/// <summary>
/// Base for immutable value objects compared by structural equality.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj) => obj is ValueObject other && Equals(other);

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, (hash, component) => HashCode.Combine(hash, component?.GetHashCode() ?? 0));

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
