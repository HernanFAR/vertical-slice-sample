using System.Collections.Concurrent;
using VSlices.Base.Responses;
using VSlices.Core.Events.ReflectionPublisher.Internals;
using VSlices.Core.Events.ReflectionPublisher.Strategies;

namespace VSlices.Core.Events.ReflectionPublisher;

/// <summary>
/// Sends a request through the VSlices pipeline to be handled by a many handlers, using reflection
/// </summary>
public class ReflectionPublisher : IPublisher
{
    internal static readonly ConcurrentDictionary<Type, AbstractHandlerWrapper> RequestHandlers = new();

    readonly IServiceProvider _serviceProvider;
    readonly IPublishingStrategy _strategy;

    /// <summary>
    /// Creates a new instance of <see cref="ReflectionPublisher"/>
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/> used to resolve handlers</param>
    /// <param name="strategy">Strategy</param>
    public ReflectionPublisher(IServiceProvider serviceProvider, IPublishingStrategy strategy)
    {
        _serviceProvider = serviceProvider;
        _strategy = strategy;
    }

    /// <inheritdoc />
    public async ValueTask PublishAsync(IEvent request, CancellationToken cancellationToken = default)
    {
        var handler = (AbstractHandlerWrapper<Success>)RequestHandlers.GetOrAdd(
            request.GetType(),
            requestType =>
            {
                var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, typeof(Success));
                var wrapper = Activator.CreateInstance(wrapperType, _strategy)
                              ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
                return (AbstractHandlerWrapper)wrapper;
            });

        await handler.HandleAsync(request, _serviceProvider, cancellationToken);
    }
}
