using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
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
        IEnumerable<IEventHandler<TRequest>> handlers = serviceProvider.GetServices<IEventHandler<TRequest>>();

        Eff<VSlicesRuntime, Unit>[] delegates =
            handlers.Select(handler =>
                            {
                                Eff<VSlicesRuntime, Unit> handlerEffect =
                                    handler.Define((TRequest)@event);

                                IEnumerable<IPipelineBehavior<TRequest, Unit>>
                                    pipelines = serviceProvider
                                                .GetServices<IPipelineBehavior<TRequest, Unit>>()
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
