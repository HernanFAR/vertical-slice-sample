using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using VSlices.Core.Events.Internals;
using VSlices.Core.Events.Strategies;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events;

/// <summary>
/// Sends a request through the VSlices pipeline to handle by a many handlers, using reflection.
/// </summary>
/// 
[RequiresDynamicCode("This class uses Type.MakeGenericType to create RequestHandlerWrapper instances")]
public sealed class ReflectionEventRunner(
    IServiceProvider serviceProvider, 
    IPublishingStrategy strategy)
    : IEventRunner
{
    internal static readonly ConcurrentDictionary<Type, AbstractHandlerWrapper> RequestHandlers = new();

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IPublishingStrategy _strategy = strategy;

    /// <inheritdoc />
    public Fin<Unit> Publish(IEvent request, HandlerRuntime runtime)
    {
        AbstractHandlerWrapper handler = RequestHandlers.GetOrAdd(
            request.GetType(),
            requestType =>
            {
                Type wrapperType = typeof(RequestHandlerWrapper<>).MakeGenericType(requestType);
                object wrapper = Activator.CreateInstance(wrapperType)
                                 ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
                return (AbstractHandlerWrapper)wrapper;
            });

        return handler.Handle(request, runtime, _serviceProvider, _strategy);
    }
}
