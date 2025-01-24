using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Base.Definitions;
using VSlices.Core.Events.Strategies;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events.Internals;

internal abstract class AbstractHandlerWrapper
{
    public abstract Fin<Unit> Handle(
        IEvent @event,
        IServiceProvider serviceProvider,
        IPublishingStrategy strategy,
        CancellationToken cancellationToken);
}

internal class RequestHandlerWrapper<TRequest> : AbstractHandlerWrapper
    where TRequest : IEvent
{
    public override Fin<Unit> Handle(
        IEvent @event,
        IServiceProvider serviceProvider,
        IPublishingStrategy strategy,
        CancellationToken cancellationToken)
    {
        IEnumerable<IBehavior<TRequest, Unit>> handlers = serviceProvider.GetServices<IBehavior<TRequest, Unit>>();

        Eff<VSlicesRuntime, Unit>[] delegates =
            handlers.Select(handler =>
                            {
                                Eff<VSlicesRuntime, Unit> handlerEffect =
                                    handler.Define((TRequest)@event);

                                Type handlerBehaviorChainType = typeof(BehaviorInterceptorChain<>)
                                    .MakeGenericType(handler.GetType());

                                HandlerBehaviorChain? pipelineChain = (HandlerBehaviorChain?)serviceProvider.GetService(handlerBehaviorChainType);

                                var pipelines = pipelineChain is null
                                    ? []
                                    : pipelineChain.Behaviors
                                                   .Select(serviceProvider.GetService)
                                                   .Cast<IBehaviorInterceptor<TRequest, Unit>>()
                                                   .Reverse();

                                return pipelines.Aggregate(handlerEffect,
                                                           (current, behavior) =>
                                                               behavior.Define((TRequest)@event,
                                                                               current));

                            })
                    .ToArray();

        return strategy.Handle(delegates, serviceProvider, cancellationToken);

    }
}
