using VSlices.Core;
using VSlices.Core.Events;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="IPublisher"/>, <see cref="IEventQueue"/> and
/// <see cref="IEventListener"/>
/// </summary>
public static class EventExtensions
{
    /// <summary>
    /// Adds a <see cref="IPublisher"/> implementation to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <typeparam name="T">Implementation of the <see cref="IPublisher"/></typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddPublisher<T>(this IServiceCollection services)
        where T : IPublisher
    {
        return services.AddPublisher(typeof(T));
    }

    /// <summary>
    /// Add the specified type as <see cref="IPublisher"/> to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">ServiceCollection</param>
    /// <param name="type">Implementation Type</param>
    /// <returns>ServiceCollection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddPublisher(this IServiceCollection services,
        Type type)
    {
        if (!typeof(IPublisher).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"{type.FullName} does not implement {typeof(IPublisher).FullName}");
        }

        return services.AddScoped(typeof(IPublisher), type);
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
    /// <typeparam name="T">Implementation Type</typeparam>
    /// <param name="services">Service Collection</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddEventListenerService<T>(this IServiceCollection services)
    {
        return services.AddEventListenerService(typeof(T));
    }

    /// <summary>
    /// Adds a hosted service that will listen for events in the background
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="type">Implementation Type</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddEventListenerService(this IServiceCollection services,
        Type type)
    {
        if (!typeof(IEventListener).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"{type.FullName} does not implement {typeof(IEventListener).FullName}");
        }

        return services.AddSingleton(typeof(IEventListener), type);
    }
}
