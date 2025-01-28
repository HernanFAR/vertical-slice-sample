using VSlices.Core.UseCases;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection" /> extensions for <see cref="IRequestRunner"/>
/// </summary>
public static class RequestRunnerExtensions
{
    /// <summary>
    /// Adds a <see cref="IRequestRunner"/> implementation to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <typeparam name="T">Implementation of the <see cref="IRequestRunner"/></typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddRequestRunner<T>(this IServiceCollection services)
        where T : IRequestRunner
    {
        services.AddSingleton(typeof(IRequestRunner), typeof(T));

        return services;
    }
}
