using LanguageExt;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.CrossCutting.StreamPipeline;

namespace VSlices.Core.Stream.Internals;

internal abstract class AbstractStreamRunnerWrapper
{
    public abstract ValueTask<Fin<object?>> HandleAsync(
        object request,
        Runtime runtime,
        IServiceProvider serviceProvider);
}

internal abstract class AbstractStreamRunnerWrapper<TResult> : AbstractStreamRunnerWrapper
{
    public abstract ValueTask<Fin<IAsyncEnumerable<TResult>>> HandleAsync(
        IStream<TResult> request,
        Runtime runtime,
        IServiceProvider serviceProvider);
}

internal class StreamRunnerWrapper<TRequest, TResult> : AbstractStreamRunnerWrapper<TResult>
    where TRequest : IStream<TResult>
{
    public override async ValueTask<Fin<object?>> HandleAsync(object request, Runtime runtime, IServiceProvider serviceProvider)
    {
        return await HandleAsync((IStream<TResult>)request, runtime, serviceProvider);
    }

    public override ValueTask<Fin<IAsyncEnumerable<TResult>>> HandleAsync(IStream<TResult> request, Runtime runtime, 
        IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IStreamHandler<TRequest, TResult>>();

        Aff<Runtime, IAsyncEnumerable<TResult>> handlerEffect = handler.Define((TRequest)request);

        IEnumerable<IStreamPipelineBehavior<TRequest, TResult>> pipelines = serviceProvider
            .GetServices<IStreamPipelineBehavior<TRequest, TResult>>()
            .Reverse();

        Aff<Runtime, IAsyncEnumerable<TResult>> effectChain = pipelines.Aggregate(handlerEffect, 
                (current, behavior) => behavior.Define((TRequest)request, current));

        return effectChain.Run(runtime);
    }
}
