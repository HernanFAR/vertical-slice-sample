using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Core.Events.Strategies;
using VSlices.CrossCutting.Pipeline;

namespace VSlices.Core.Events.Internals;

internal abstract class AbstractHandlerWrapper
{
    public abstract Fin<Unit> Handle(
        IFeature<Unit> request,
        IServiceProvider serviceProvider,
        IPublishingStrategy strategy,
        CancellationToken cancellationToken);
}

internal class RequestHandlerWrapper<TRequest> : AbstractHandlerWrapper
    where TRequest : IFeature<Unit>
{
    public override Fin<Unit> Handle(
        IFeature<Unit> request,
        IServiceProvider serviceProvider,
        IPublishingStrategy strategy, 
        CancellationToken cancellationToken)
    {
        var runtime = serviceProvider.GetRequiredService<VSlicesRuntime>();
        IEnumerable<IHandler<TRequest, Unit>> handlers = serviceProvider
            .GetServices<IHandler<TRequest, Unit>>();

        Eff<VSlicesRuntime, Unit>[] delegates =
            handlers.Select(handler =>
                            {
                                Eff<VSlicesRuntime, Unit> handlerEffect =
                                    handler.Define((TRequest)request);

                                IEnumerable<IPipelineBehavior<TRequest, Unit>>
                                    pipelines = serviceProvider
                                                .GetServices<IPipelineBehavior<TRequest, Unit>>()
                                                .Reverse();

                                return pipelines.Aggregate(handlerEffect,
                                                           (current, behavior) =>
                                                               behavior.Define((TRequest)request,
                                                                               current));

                            })
                    .ToArray();

        return strategy.Handle(delegates, runtime, cancellationToken);

    }
}
