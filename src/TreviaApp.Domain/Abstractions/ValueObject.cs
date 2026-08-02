namespace TreviaApp.Domain.Abstractions;

/// <summary>
/// Represents the ValueObject domain entity.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Executes Get Equality Components.
    /// </summary>
    public abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Executes Equals.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        return obj is ValueObject other && ValuesEqual(other);
    }

    /// <summary>
    /// Executes Equals.
    /// </summary>
    public bool Equals(ValueObject? other) => other is not null && ValuesEqual(other);

    private bool ValuesEqual(ValueObject other) => GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    /// <summary>
    /// Executes Get Hash Code.
    /// </summary>
    public override int GetHashCode() => GetEqualityComponents()
        .Aggregate(default(HashCode), (hc, obj) =>
        {
            hc.Add(obj);
            return hc;
        })
        .ToHashCode();

    /// <summary>
    /// Compares two value objects for structural equality.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    /// <summary>
    /// Compares two value objects for structural inequality.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}
