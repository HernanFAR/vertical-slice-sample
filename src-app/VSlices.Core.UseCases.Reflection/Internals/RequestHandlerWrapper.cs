using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.CrossCutting.Pipeline;

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
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        Eff<VSlicesRuntime, TResponse> handlerEffect = handler.Define((TRequest)request);

        IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelines = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse();

        Eff<VSlicesRuntime, TResponse> effectChain = pipelines
            .Aggregate(handlerEffect,
                       (next, behavior) => behavior.Define((TRequest)request, next));

        using var scope   = serviceProvider.CreateScope();
        var       runtime = scope.ServiceProvider.GetRequiredService<VSlicesRuntime>();

        return effectChain.Run(runtime, cancellationToken);
    }
}
