using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Events.Internals;
using VSlices.Core.Events.Strategies;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events;

/// <summary>
/// Sends a input through the VSlices pipeline to handle by a many handlers, using reflection.
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
    public Fin<Unit> Publish(IEvent request, CancellationToken cancellationToken = default)
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


        using IServiceScope scope = _serviceProvider.CreateScope();

        return handler.Handle(request, scope.ServiceProvider, _strategy, cancellationToken);
    }
}
