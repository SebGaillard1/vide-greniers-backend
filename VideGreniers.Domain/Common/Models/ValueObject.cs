namespace VideGreniers.Domain.Common.Models;

/// <summary>
/// Base class for value objects following DDD principles
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Gets the equality components for this value object
    /// </summary>
    /// <returns>The components that determine equality</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is equal to the current object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Returns a hash code for the current object
    /// </summary>
    /// <returns>A hash code for the current object</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Determines whether two value objects are equal
    /// </summary>
    /// <param name="left">The first value object to compare</param>
    /// <param name="right">The second value object to compare</param>
    /// <returns>true if the value objects are equal; otherwise, false</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two value objects are not equal
    /// </summary>
    /// <param name="left">The first value object to compare</param>
    /// <param name="right">The second value object to compare</param>
    /// <returns>true if the value objects are not equal; otherwise, false</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}