using VSlices.Core.Stream;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection" /> extensions for <see cref="IStreamRunner"/>
/// </summary>
public static class StreamRunnerExtensions
{
    /// <summary>
    /// Adds a <see cref="IStreamRunner"/> implementation to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <typeparam name="T">Implementation of the <see cref="IStreamRunner"/></typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddStreamRunner<T>(this IServiceCollection services)
        where T : IStreamRunner
    {
        services.AddSingleton(typeof(IStreamRunner), typeof(T));

        return services;
    }
}
