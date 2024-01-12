using VSlices.Core.Events;
using VSlices.Core.Events.Strategies;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="ReflectionPublisher"/>.
/// </summary>
public static class ReflectionPublisherExtensions
{
    /// <summary>
    /// Add <see cref="ReflectionPublisher"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>Default publishing strategy is <see cref="AwaitInParallelStrategy"/></remarks>
    /// <param name="services">Service Collection</param>
    /// <param name="strategy">Publishing strategy</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddReflectionPublisher(this IServiceCollection services, 
        IPublishingStrategy? strategy = null)
    {
        strategy ??= new AwaitInParallelStrategy();

        services.AddPublisher<ReflectionPublisher>();
        services.AddSingleton(strategy);

        return services;
    }
}
