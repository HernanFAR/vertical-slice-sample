using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.CrossCutting.StreamPipeline;

namespace VSlices.Core.Stream.Internals;

internal abstract class AbstractStreamRunnerWrapper
{
    public abstract Fin<object?> Handle(
        object request,
        HandlerRuntime runtime,
        IServiceProvider serviceProvider);
}

internal abstract class AbstractStreamRunnerWrapper<TResult> : AbstractStreamRunnerWrapper
{
    public abstract Fin<IAsyncEnumerable<TResult>> Handle(
        IStream<TResult> request,
        HandlerRuntime runtime,
        IServiceProvider serviceProvider);
}

internal class StreamRunnerWrapper<TRequest, TResult> : AbstractStreamRunnerWrapper<TResult>
    where TRequest : IStream<TResult>
{
    public override Fin<object?> Handle(object request, 
                                        HandlerRuntime runtime, 
                                        IServiceProvider serviceProvider) => 
        Handle((IStream<TResult>)request, runtime, serviceProvider);

    public override Fin<IAsyncEnumerable<TResult>> Handle(IStream<TResult> request, 
                                                          HandlerRuntime runtime, 
                                                          IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IStreamHandler<TRequest, TResult>>();

        Eff<HandlerRuntime, IAsyncEnumerable<TResult>> handlerEffect = 
            handler.Define((TRequest)request);

        IEnumerable<IStreamPipelineBehavior<TRequest, TResult>> pipelines = serviceProvider
            .GetServices<IStreamPipelineBehavior<TRequest, TResult>>()
            .Reverse();

        Eff<HandlerRuntime, IAsyncEnumerable<TResult>> effectChain = 
            pipelines.Aggregate(handlerEffect, 
                                (current, behavior) => behavior.Define((TRequest)request, current));

        return effectChain.Run(runtime);
    }
}
