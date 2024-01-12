using VSlices.Core.Events;
using VSlices.Core.Events.HostedEventListener;
using VSlices.Core.Events.HostedEventListener.Configurations;

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
    public static IServiceCollection AddHostedEventListener(this IServiceCollection services,
        Action<HostedEventListenerConfiguration>? configAct = null)
    {
        services.AddHostedService<HostedEventListener>();

        var config = new HostedEventListenerConfiguration();
        configAct?.Invoke(config);

        services.AddSingleton(config);

        return services;
    }
}
