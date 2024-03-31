using VSlices.Core.Events;
using VSlices.Core.Events.Configurations;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="HostedEventListener"/>
/// </summary>
public static class HostedEventListenerExtensions
{
    /// <summary>
    /// Adds a hosted service that will listen for events in the background
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="configAct">Action to configure the service</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddHostedEventListener<TEventListenerCore>(this IServiceCollection services,
        Action<EventListenerConfiguration>? configAct = null)
        where TEventListenerCore : IEventListenerCore
    {
        return services
            .AddEventListener<TEventListenerCore>(configAct)
            .AddHostedService<HostedEventListener>();
    }

    /// <summary>
    /// Adds a hosted service that will listen for events in the background
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="configAct">Action to configure the service</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddDefaultHostedEventListener(this IServiceCollection services,
        Action<EventListenerConfiguration>? configAct = null)
    {
        return services
            .AddEventListener<EventListenerCore>(configAct)
            .AddHostedService<HostedEventListener>();
    }
}
