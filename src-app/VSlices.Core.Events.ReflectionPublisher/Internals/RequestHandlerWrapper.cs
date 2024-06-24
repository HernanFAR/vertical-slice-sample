﻿using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
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
        Runtime runtime,
        IServiceProvider serviceProvider);
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
        IFeature<Unit> request,
        Runtime runtime, 
        IServiceProvider serviceProvider)
    {
        IEnumerable<IHandler<TRequest, Unit>> handlers = serviceProvider
            .GetServices<IHandler<TRequest, Unit>>();

        Aff<Runtime, Unit>[] handlerDelegates = handlers.Select(handler =>
            {
                Aff<Runtime, Unit> handlerEffect = handler.Define((TRequest)request);

                IEnumerable<IPipelineBehavior<TRequest, Unit>> pipelines = serviceProvider
                    .GetServices<IPipelineBehavior<TRequest, Unit>>()
                    .Reverse();

                return pipelines.Aggregate(handlerEffect,
                    (current, behavior) => behavior.Define((TRequest)request, current));

            })
            .ToArray();

        return await _strategy.HandleAsync(handlerDelegates, runtime);
    }
}
