namespace TreviaApp.Domain.Abstractions;

public abstract class ValueObject : IEquatable<ValueObject>
{
    public abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        return obj is ValueObject other && ValuesEqual(other);
    }

    public bool Equals(ValueObject? other) => other is not null && ValuesEqual(other);

    private bool ValuesEqual(ValueObject other) => GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override int GetHashCode() => GetEqualityComponents()
        .Aggregate(default(HashCode), (hc, obj) =>
        {
            hc.Add(obj);
            return hc;
        })
        .ToHashCode();

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}
