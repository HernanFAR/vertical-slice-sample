using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.CrossCutting.Pipeline;

namespace VSlices.Core.UseCases.Internals;

internal abstract class AbstractRequestRunnerWrapper
{
    public abstract ValueTask<Fin<object?>> HandleAsync(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal abstract class AbstractRequestRunnerWrapper<TResponse> : AbstractRequestRunnerWrapper
{
    public abstract ValueTask<Fin<TResponse>> HandleAsync(
        IFeature<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal class RequestRunnerWrapper<TRequest, TResponse> : AbstractRequestRunnerWrapper<TResponse>
    where TRequest : IFeature<TResponse>
{
    public override async ValueTask<Fin<object?>> HandleAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return await HandleAsync((IFeature<TResponse>)request, serviceProvider, cancellationToken);
    }

    public override ValueTask<Fin<TResponse>> HandleAsync(IFeature<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IHandler<TRequest, TResponse>>();

        Aff<TResponse> handlerEffect = handler.Define((TRequest)request, cancellationToken);

        IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelines = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse();

        Aff<TResponse> effectChain = pipelines.Aggregate(handlerEffect, 
                (current, behavior) => behavior.Define((TRequest)request, current, cancellationToken));

        return effectChain.Run();
    }
}
