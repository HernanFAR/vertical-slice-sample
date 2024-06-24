using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.CrossCutting.StreamPipeline;

namespace VSlices.Core.Stream.Internals;

internal abstract class AbstractStreamRunnerWrapper
{
    public abstract ValueTask<Fin<object?>> HandleAsync(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal abstract class AbstractStreamRunnerWrapper<TResult> : AbstractStreamRunnerWrapper
{
    public abstract ValueTask<Fin<IAsyncEnumerable<TResult>>> HandleAsync(
        IStream<TResult> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal class StreamRunnerWrapper<TRequest, TResult> : AbstractStreamRunnerWrapper<TResult>
    where TRequest : IStream<TResult>
{
    public override async ValueTask<Fin<object?>> HandleAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return await HandleAsync((IStream<TResult>)request, serviceProvider, cancellationToken);
    }

    public override ValueTask<Fin<IAsyncEnumerable<TResult>>> HandleAsync(IStream<TResult> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IStreamHandler<TRequest, TResult>>();

        Aff<IAsyncEnumerable<TResult>> handlerEffect = handler.Define((TRequest)request, cancellationToken);

        IEnumerable<IStreamPipelineBehavior<TRequest, TResult>> pipelines = serviceProvider
            .GetServices<IStreamPipelineBehavior<TRequest, TResult>>()
            .Reverse();

        Aff<IAsyncEnumerable<TResult>> effectChain = pipelines.Aggregate(handlerEffect, 
                (current, behavior) => behavior.Define((TRequest)request, current, cancellationToken));

        return effectChain.Run();
    }
}
