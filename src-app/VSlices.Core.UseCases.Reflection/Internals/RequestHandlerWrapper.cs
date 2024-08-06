using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;

namespace VSlices.Core.UseCases.Internals;

internal abstract class AbstractRequestRunnerWrapper
{
    public abstract Fin<object?> Handle(
        object request,
        IServiceProvider serviceProvider, 
        CancellationToken cancellationToken);
}

internal abstract class AbstractRequestRunnerWrapper<TResponse> : AbstractRequestRunnerWrapper
{
    public abstract Fin<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider, 
        CancellationToken cancellationToken);
}

internal class RequestRunnerWrapper<TRequest, TResponse> : AbstractRequestRunnerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Fin<object?> Handle(object request, 
                                        IServiceProvider serviceProvider, 
                                        CancellationToken cancellationToken)
    {
        return Handle((IRequest<TResponse>)request, serviceProvider, cancellationToken);
    }

    public override Fin<TResponse> Handle(IRequest<TResponse> request, 
                                          IServiceProvider serviceProvider, 
                                          CancellationToken cancellationToken)
    {
        var handler       = serviceProvider
            .GetRequiredService<IHandler<TRequest, TResponse>>();
        
        var handlerBehaviorChainType = typeof(HandlerBehaviorChain<>)
            .MakeGenericType(handler.GetType());

        var pipelineChain = (HandlerBehaviorChain)serviceProvider.GetRequiredService(handlerBehaviorChainType);

        var pipelines = pipelineChain.Behaviors
                                     .Select(serviceProvider.GetService)
                                     .Cast<IPipelineBehavior<TRequest, TResponse>>()
                                     .Reverse();

        Eff<VSlicesRuntime, TResponse> handlerEffect = handler.Define((TRequest)request);

        Eff<VSlicesRuntime, TResponse> effectChain = pipelines
            .Aggregate(handlerEffect,
                       (next, behavior) => behavior.Define((TRequest)request, next));

        using var scope   = serviceProvider.CreateScope();
        var       runtime = scope.ServiceProvider.GetRequiredService<VSlicesRuntime>();

        return effectChain.Run(runtime, cancellationToken);
    }
}
