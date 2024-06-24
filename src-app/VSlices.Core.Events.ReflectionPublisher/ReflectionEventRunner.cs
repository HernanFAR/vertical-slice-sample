using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using LanguageExt.SysX.Live;
using VSlices.Core.Events.Internals;
using VSlices.Core.Events.Strategies;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events;

/// <summary>
/// Sends a request through the VSlices pipeline to be handled by a many handlers, using reflection
/// </summary>
/// 
[RequiresDynamicCode("This class uses Type.MakeGenericType to create RequestHandlerWrapper instances")]
public class ReflectionEventRunner : IEventRunner
{
    internal static readonly ConcurrentDictionary<Type, AbstractHandlerWrapper> RequestHandlers = new();

    readonly IServiceProvider _serviceProvider;
    readonly IPublishingStrategy _strategy;

    /// <summary>
    /// Creates a new instance of <see cref="ReflectionEventRunner"/>
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/> used to resolve handlers</param>
    /// <param name="strategy">Strategy</param>
    public ReflectionEventRunner(IServiceProvider serviceProvider, IPublishingStrategy strategy)
    {
        _serviceProvider = serviceProvider;
        _strategy = strategy;
    }

    /// <inheritdoc />
    public async ValueTask<Fin<Unit>> PublishAsync(IEvent request, Runtime runtime)
    {
        AbstractHandlerWrapper handler = RequestHandlers.GetOrAdd(
            request.GetType(),
            requestType =>
            {
                Type wrapperType = typeof(RequestHandlerWrapper<>).MakeGenericType(requestType);
                object wrapper = Activator.CreateInstance(wrapperType, _strategy)
                                 ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
                return (AbstractHandlerWrapper)wrapper;
            });

        return await handler.HandleAsync(request, runtime, _serviceProvider);
    }
}
