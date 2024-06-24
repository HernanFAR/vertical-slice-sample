using LanguageExt;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.CrossCutting.Pipeline;

namespace VSlices.Core.UseCases.Internals;

internal abstract class AbstractRequestRunnerWrapper
{
    public abstract ValueTask<Fin<object?>> HandleAsync(
        object request,
        Runtime runtime,
        IServiceProvider serviceProvider);
}

internal abstract class AbstractRequestRunnerWrapper<TResponse> : AbstractRequestRunnerWrapper
{
    public abstract ValueTask<Fin<TResponse>> HandleAsync(
        IFeature<TResponse> request,
        Runtime runtime,
        IServiceProvider serviceProvider);
}

internal class RequestRunnerWrapper<TRequest, TResponse> : AbstractRequestRunnerWrapper<TResponse>
    where TRequest : IFeature<TResponse>
{
    public override async ValueTask<Fin<object?>> HandleAsync(object request, Runtime runtime, IServiceProvider serviceProvider)
    {
        return await HandleAsync((IFeature<TResponse>)request, runtime, serviceProvider);
    }

    public override ValueTask<Fin<TResponse>> HandleAsync(IFeature<TResponse> request, Runtime runtime, 
        IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IHandler<TRequest, TResponse>>();

        Aff<Runtime, TResponse> handlerEffect = handler.Define((TRequest)request);

        IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelines = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse();

        Aff<Runtime, TResponse> effectChain = pipelines.Aggregate(handlerEffect, 
                (next, behavior) => behavior.Define((TRequest)request, next));

        return effectChain.Run(runtime);
    }
}
