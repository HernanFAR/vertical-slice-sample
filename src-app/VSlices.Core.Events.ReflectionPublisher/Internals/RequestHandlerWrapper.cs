using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.Events.Strategies;
using VSlices.CrossCutting;

namespace VSlices.Core.Events.Internals;

internal abstract class AbstractHandlerWrapper
{
    public abstract ValueTask HandleAsync(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal abstract class AbstractHandlerWrapper<TResponse> : AbstractHandlerWrapper
{
    public abstract ValueTask HandleAsync(
        IBaseRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal class RequestHandlerWrapper<TRequest, TResponse> : AbstractHandlerWrapper<TResponse>
    where TRequest : IBaseRequest<TResponse>
{
    private readonly IPublishingStrategy _strategy;

    public RequestHandlerWrapper(IPublishingStrategy strategy)
    {
        _strategy = strategy;
    }

    public override async ValueTask HandleAsync(
        object request, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        await HandleAsync((IBaseRequest<TResponse>)request, serviceProvider, cancellationToken);

    public override async ValueTask HandleAsync(
        IBaseRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetServices<IHandler<TRequest, TResponse>>();

        var handlerDelegates = handlers.Select(handler =>
            {
                return serviceProvider
                    .GetServices<IPipelineBehavior<TRequest, TResponse>>()
                    .Reverse()
                    .Aggregate((RequestHandlerDelegate<TResponse>)Handler,
                        (next, pipeline) => () => pipeline.HandleAsync((TRequest)request, next, cancellationToken));

                ValueTask<Result<TResponse>> Handler()
                {
                    return handler.HandleAsync((TRequest)request, cancellationToken);
                }
            })
            .ToArray();

        await _strategy.HandleAsync(handlerDelegates);

    }
}
