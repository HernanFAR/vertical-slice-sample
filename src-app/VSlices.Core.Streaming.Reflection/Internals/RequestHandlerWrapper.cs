﻿using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.CrossCutting.StreamPipeline;

namespace VSlices.Core.Stream.Internals;

internal abstract class AbstractStreamRunnerWrapper
{
    public abstract Fin<object?> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal abstract class AbstractStreamRunnerWrapper<TResult> : AbstractStreamRunnerWrapper
{
    public abstract Fin<IAsyncEnumerable<TResult>> Handle(
        IStream<TResult> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal class StreamRunnerWrapper<TRequest, TResult> : AbstractStreamRunnerWrapper<TResult>
    where TRequest : IStream<TResult>
{
    public override Fin<object?> Handle(object request, 
                                        IServiceProvider serviceProvider,
                                        CancellationToken cancellationToken) => 
        Handle((IStream<TResult>)request, serviceProvider, cancellationToken);

    public override Fin<IAsyncEnumerable<TResult>> Handle(IStream<TResult> request, 
                                                          IServiceProvider serviceProvider,
                                                          CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IStreamHandler<TRequest, TResult>>();

        Eff<VSlicesRuntime, IAsyncEnumerable<TResult>> handlerEffect = 
            handler.Define((TRequest)request);

        IEnumerable<IStreamPipelineBehavior<TRequest, TResult>> pipelines = serviceProvider
            .GetServices<IStreamPipelineBehavior<TRequest, TResult>>()
            .Reverse();

        Eff<VSlicesRuntime, IAsyncEnumerable<TResult>> effectChain = 
            pipelines.Aggregate(handlerEffect, 
                                (current, behavior) => behavior.Define((TRequest)request, current));

        using var scope   = serviceProvider.CreateScope();
        var       runtime = scope.ServiceProvider.GetRequiredService<VSlicesRuntime>();

        return effectChain.Run(runtime, cancellationToken);
    }
}
