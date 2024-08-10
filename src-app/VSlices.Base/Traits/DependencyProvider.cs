using Microsoft.Extensions.DependencyInjection;

namespace VSlices.Base.Traits;

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
    public IO<T> Provide<T>()
        where T : notnull
        => lift(_provider.GetRequiredService<T>);

    /// <summary>
    /// Gets an optional service
    /// </summary>
    /// <typeparam name="T">The specified type to return</typeparam>
    public IO<Option<T>> ProvideOptional<T>()
        where T : notnull
        => lift(() => Optional(_provider.GetService<T>()));

}