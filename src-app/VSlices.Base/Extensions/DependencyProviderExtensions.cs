// ReSharper disable once CheckNamespace
using VSlices.Base.Traits;

namespace VSlices.Base.Extensions;

/// <summary>
/// Extensions to use the dependency provider 
/// </summary>
public static class DependencyProviderExtensions<TMonad, TRuntime>
    where TMonad : Monad<TMonad>
    where TRuntime : Has<TMonad, DependencyProvider>
{
    /// <summary>
    /// Gets a required service
    /// </summary>
    public static K<TMonad, T> Provide<T>()
        where T : notnull =>
        from t in TRuntime.Ask
        from x in t.Provide<T>()
        select x;

    /// <summary>
    /// Gets an optional service
    /// </summary>
    public static K<TMonad, Option<T>> ProvideOptional<T>()
        where T : notnull =>
        from t in TRuntime.Ask
        from x in t.ProvideOptional<T>()
        select x;
}