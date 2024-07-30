using VSlices.Core.Events;
using VSlices.Core.Events.Configurations;
using VSlices.CrossCutting.BackgroundTaskListener;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="IEventRunner"/>, <see cref="IEventQueue"/> and
/// <see cref="EventListenerBackgroundTask"/>
/// </summary>
public static class EventExtensions
{
    /// <summary>
    /// Adds a <see cref="IEventRunner"/> implementation to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <typeparam name="T">Implementation of the <see cref="IEventRunner"/></typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddEventRunner<T>(this IServiceCollection services)
        where T : IEventRunner
    {
        return services.AddEventRunner(typeof(T));
    }

    /// <summary>
    /// Add the specified type as <see cref="IEventRunner"/> to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">ServiceCollection</param>
    /// <param name="type">Implementation Type</param>
    /// <returns>ServiceCollection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddEventRunner(this IServiceCollection services,
        Type type)
    {
        if (!typeof(IEventRunner).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"{type.FullName} does not implement {typeof(IEventRunner).FullName}");
        }

        return services.AddSingleton(typeof(IEventRunner), type);
    }

    /// <summary>
    /// Adds a <see cref="IEventQueue"/> implementation to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <remarks>
    /// It also adds it as <see cref="IEventQueueWriter"/> and <see cref="IEventQueueReader"/> implementation
    /// </remarks>
    /// <typeparam name="T">Implementation of the <see cref="IEventQueue"/></typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddEventQueue<T>(this IServiceCollection services)
        where T : IEventQueue
    {
        return services.AddEventQueue(typeof(T));
    }

    /// <summary>
    /// Adds the specified type as <see cref="IEventQueue"/> to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <remarks>
    /// It also adds it as <see cref="IEventQueueWriter"/> and <see cref="IEventQueueReader"/> implementation
    /// </remarks>
    /// <param name="type">Implementation of the <see cref="IEventQueue"/></param>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddEventQueue(this IServiceCollection services, Type type)
    {
        if (!typeof(IEventQueue).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"{type.FullName} does not implement {typeof(IEventQueue).FullName}");
        }

        services.AddSingleton(typeof(IEventQueue), type);
        services.AddSingleton<IEventQueueWriter>(s => s.GetRequiredService<IEventQueue>());
        services.AddSingleton<IEventQueueReader>(s => s.GetRequiredService<IEventQueue>());

        return services;
    }

    /// <summary>
    /// Adds a hosted service that will listen for events in the background
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="configAction">Configuration action</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddEventListener(this IServiceCollection services,
        Action<EventListenerConfiguration>? configAction = null)
    {
        EventListenerConfiguration config = new();

        configAction?.Invoke(config);

        return services.AddSingleton<IBackgroundTask, EventListenerBackgroundTask>()
            .AddSingleton(config);
    }
}
