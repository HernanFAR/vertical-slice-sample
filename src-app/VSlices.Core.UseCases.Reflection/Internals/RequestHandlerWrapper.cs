using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Base.Definitions;

namespace VSlices.Core.UseCases.Internals;

internal abstract class AbstractRequestRunnerWrapper
{
    public abstract Fin<object?> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal abstract class AbstractRequestRunnerWrapper<TOut> : AbstractRequestRunnerWrapper
{
    public abstract Fin<TOut> Handle(
        IInput<TOut> input,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal class RequestRunnerWrapper<TIn, TOut> : AbstractRequestRunnerWrapper<TOut>
    where TIn : IInput<TOut>
{
    public override Fin<object?> Handle(object request,
                                        IServiceProvider serviceProvider,
                                        CancellationToken cancellationToken) => Handle((IInput<TOut>)request, serviceProvider, cancellationToken);

    public override Fin<TOut> Handle(IInput<TOut> input,
                                          IServiceProvider serviceProvider,
                                          CancellationToken cancellationToken)
    {
        IBehavior<TIn, TOut> handler = serviceProvider
            .GetRequiredService<IBehavior<TIn, TOut>>();

        Type handlerBehaviorChainType = typeof(BehaviorInterceptorChain<>)
            .MakeGenericType(handler.GetType());

        HandlerBehaviorChain? pipelineChain = (HandlerBehaviorChain?)serviceProvider.GetService(handlerBehaviorChainType);

        IEnumerable<IBehaviorInterceptor<TIn, TOut>> pipelines = pipelineChain is null
            ? []
            : pipelineChain.Behaviors
                           .Select(serviceProvider.GetService)
                           .Cast<IBehaviorInterceptor<TIn, TOut>>()
                           .Reverse();

        Eff<VSlicesRuntime, TOut> handlerEffect = handler.Define((TIn)input);

        Eff<VSlicesRuntime, TOut> effectChain = pipelines
            .Aggregate(handlerEffect,
                       (next, behavior) => behavior.Define((TIn)input, next));

        using IServiceScope scope = serviceProvider.CreateScope();
        VSlicesRuntime runtime = scope.ServiceProvider.GetRequiredService<VSlicesRuntime>();

        return effectChain.Run(runtime, cancellationToken);
    }
}
