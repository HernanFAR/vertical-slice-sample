using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.CrossCutting.Pipeline;

namespace VSlices.Core.UseCases.Internals;

internal abstract class AbstractRequestRunnerWrapper
{
    public abstract Fin<object?> Handle(
        object request,
        HandlerRuntime runtime,
        IServiceProvider serviceProvider);
}

internal abstract class AbstractRequestRunnerWrapper<TResponse> : AbstractRequestRunnerWrapper
{
    public abstract Fin<TResponse> Handle(
        IFeature<TResponse> request,
        HandlerRuntime runtime,
        IServiceProvider serviceProvider);
}

internal class RequestRunnerWrapper<TRequest, TResponse> : AbstractRequestRunnerWrapper<TResponse>
    where TRequest : IFeature<TResponse>
{
    public override Fin<object?> Handle(object request, HandlerRuntime runtime, IServiceProvider serviceProvider)
    {
        return Handle((IFeature<TResponse>)request, runtime, serviceProvider);
    }

    public override Fin<TResponse> Handle(IFeature<TResponse> request, HandlerRuntime runtime, 
        IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IHandler<TRequest, TResponse>>();

        Eff<HandlerRuntime, TResponse> handlerEffect = handler.Define((TRequest)request);

        IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelines = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse();

        Eff<HandlerRuntime, TResponse> effectChain = pipelines
            .Aggregate(handlerEffect,
                       (next, behavior) => behavior.Define((TRequest)request, next));

        return effectChain.Run(runtime, runtime.EnvIO);
    }
}
