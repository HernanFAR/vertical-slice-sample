using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Core.Events.Strategies;
using VSlices.CrossCutting.Pipeline;

namespace VSlices.Core.Events.Internals;

internal abstract class AbstractHandlerWrapper
{
    public abstract ValueTask<Fin<Unit>> HandleAsync(
        IFeature<Unit> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal class RequestHandlerWrapper<TRequest> : AbstractHandlerWrapper
    where TRequest : IFeature<Unit>
{
    readonly IPublishingStrategy _strategy;

    public RequestHandlerWrapper(IPublishingStrategy strategy)
    {
        _strategy = strategy;
    }

    public override async ValueTask<Fin<Unit>> HandleAsync(
        IFeature<Unit> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        IEnumerable<IHandler<TRequest, Unit>> handlers = serviceProvider
            .GetServices<IHandler<TRequest, Unit>>();

        Aff<Unit>[] handlerDelegates = handlers.Select(handler =>
            {
                Aff<Unit> handlerEffect = handler.Define((TRequest)request, cancellationToken);

                IEnumerable<IPipelineBehavior<TRequest, Unit>> pipelines = serviceProvider
                    .GetServices<IPipelineBehavior<TRequest, Unit>>()
                    .Reverse();

                return pipelines.Aggregate(handlerEffect,
                    (current, behavior) => behavior.Define((TRequest)request, current, cancellationToken));

            })
            .ToArray();

        return await _strategy.HandleAsync(handlerDelegates);
    }
}
