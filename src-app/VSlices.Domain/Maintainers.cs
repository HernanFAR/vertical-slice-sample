using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using System.Reflection;

namespace VSlices.Domain;

/// <summary>
/// Base class for all maintainers
/// </summary>
public abstract class Maintainer<TMaintainer, TKey>(TKey id)
    where TMaintainer : Maintainer<TMaintainer, TKey>
    where TKey : class, IEquatable<TKey>
{
    /// <summary>
    /// The identifier of the <see cref="TMaintainer"/>
    /// </summary>
    public TKey Id { get; } = id;

    /// <summary>
    /// Finds an instance of <see cref="TMaintainer"/> using the specified <paramref name="id"/>
    /// </summary>
    /// <remarks>
    /// If the <paramref name="id"/> is not found, the method will return <see langword="None"/>
    /// </remarks>
    public static Option<TMaintainer> FindOrOption(TKey id)
    {
        IEnumerable<PropertyInfo> properties = typeof(TMaintainer)
                         .GetProperties(BindingFlags.Public | BindingFlags.Static)
                         .Where(prop => prop.PropertyType == typeof(TMaintainer));

        return properties
               .Select(prop => prop.GetValue(null))
               .Cast<TMaintainer>()
               .FirstOrDefault(root => root.Id.Equals(id));
    }

    /// <summary>
    /// Finds an instance of <see cref="TMaintainer"/> using the specified <paramref name="id"/>
    /// </summary>
    /// <remarks>
    /// If the <paramref name="id"/> is not found, the method will throw an <see cref="InvalidOperationException"/>
    /// </remarks>
    public static TMaintainer Find(TKey id)
    {
        Option<TMaintainer> value = FindOrOption(id);

        return value.ValueUnsafe() ??
               throw new InvalidOperationException(
                   $"The specified {typeof(TKey).FullName} does not correlates to an public static readonly property of {typeof(TMaintainer).FullName}");
    }
}
