using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Core.Events.Strategies;
using VSlices.Core.UseCases;
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
        IEnumerable<IHandler<TRequest, Unit>> handlers = serviceProvider.GetServices<IHandler<TRequest, Unit>>();

        Eff<VSlicesRuntime, Unit>[] delegates =
            handlers.Select(handler =>
                            {
                                Eff<VSlicesRuntime, Unit> handlerEffect =
                                    handler.Define((TRequest)@event);

                                var handlerBehaviorChainType = typeof(HandlerBehaviorChain<>)
                                    .MakeGenericType(handler.GetType());

                                var pipelineChain = (HandlerBehaviorChain?)serviceProvider.GetService(handlerBehaviorChainType);

                                var pipelines = pipelineChain is null
                                    ? []
                                    : pipelineChain.Behaviors
                                                   .Select(serviceProvider.GetService)
                                                   .Cast<IPipelineBehavior<TRequest, Unit>>()
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
