// ReSharper disable CheckNamespace
using VSlices.Core.UseCases;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection" /> extensions for <see cref="ISender"/>
/// </summary>
public static class SenderExtensions
{
    /// <summary>
    /// Adds a <see cref="ISender"/> implementation to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <typeparam name="T">Implementation of the <see cref="ISender"/></typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="lifetime">The lifetime of the <see cref="ISender"/> implemented</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddSender<T>(this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where T : ISender
    {
        services.AddScoped(typeof(ISender), typeof(T));

        return services;
    }
}
