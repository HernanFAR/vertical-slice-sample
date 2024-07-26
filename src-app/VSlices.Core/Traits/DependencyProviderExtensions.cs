using Microsoft.Extensions.DependencyInjection;

namespace VSlices.Core.Traits;

/// <summary>
/// Allows a runtime to provide dependencies
/// </summary>
public sealed class DependencyProvider(IServiceProvider provider)
{
    private readonly IServiceProvider _provider = provider;

    /// <summary>
    /// Gets a required services
    /// </summary>
    /// <typeparam name="T">The specified type to return</typeparam>
    public IO<T> Get<T>()
        where T : notnull
        => lift(_provider.GetRequiredService<T>);

    /// <summary>
    /// Gets an optional service
    /// </summary>
    /// <typeparam name="T">The specified type to return</typeparam>
    public IO<Option<T>> GetOptional<T>()
        where T : notnull
        => lift(() => Optional(_provider.GetService<T>()));

}

/// <summary>
/// Extensions to use the dependency provider 
/// </summary>
public static class DependencyProviderExtensions<TMonad, TRuntime>
    where TMonad : Stateful<TMonad, TRuntime>, Monad<TMonad>
    where TRuntime : Has<TMonad, DependencyProvider>
{
    /// <summary>
    /// Gets the dependency provider
    /// </summary>
    private static readonly K<TMonad, DependencyProvider> _trait = 
        Stateful.getsM<TMonad, TRuntime, DependencyProvider>(e => e.Trait);

    /// <summary>
    /// Gets a required service
    /// </summary>
    public static K<TMonad, T> Get<T>()
        where T : notnull => 
        from t in _trait
        from x in t.Get<T>()
        select x;

    /// <summary>
    /// Gets an optional service
    /// </summary>
    public static K<TMonad, Option<T>> GetOptional<T>()
        where T : notnull =>
        from t in _trait
        from x in t.GetOptional<T>()
        select x;   
}