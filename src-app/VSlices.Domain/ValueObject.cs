namespace VSlices.Domain;

/// <summary>
/// Base class for value objects
/// </summary>
/// <remarks> Provides a <see cref="ValueEquals"/> method</remarks>
public abstract class ValueObject
{
    /// <summary>
    /// Gets the values of the value object
    /// </summary>
    /// <remarks>Use <code>yield return val;</code></remarks>
    /// <returns>An enumeration of the properties</returns>
    protected abstract IEnumerable<object?> GetAtomicValues();

    /// <summary>
    /// Performs a value check between two value objects
    /// </summary>
    /// <param name="obj">The other value object to compare with</param>
    /// <returns>true if it has the same values, false if not</returns>
    public bool ValueEquals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        using var thisValues = GetAtomicValues().GetEnumerator();
        using var otherValues = other.GetAtomicValues().GetEnumerator();

        while (thisValues.MoveNext() && otherValues.MoveNext())
        {
            if (thisValues.Current is null ^ otherValues.Current is null)
            {
                return false;
            }

            if (thisValues.Current != null &&
                !thisValues.Current.Equals(otherValues.Current))
            {
                return false;
            }
        }

        return !thisValues.MoveNext() && !otherValues.MoveNext();
    }
}