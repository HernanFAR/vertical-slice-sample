using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.CrossCutting;

namespace VSlices.Core.UseCases.Internals;

internal abstract class AbstractHandlerWrapper
{
    public abstract ValueTask<Result<object?>> HandleAsync(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal abstract class AbstractHandlerWrapper<TResponse> : AbstractHandlerWrapper
{
    public abstract ValueTask<Result<TResponse>> HandleAsync(
        IFeature<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal class RequestHandlerWrapper<TRequest, TResponse> : AbstractHandlerWrapper<TResponse>
    where TRequest : IFeature<TResponse>
{
    public override async ValueTask<Result<object?>> HandleAsync(
        object request, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        await HandleAsync((IFeature<TResponse>)request, serviceProvider, cancellationToken);

    public override ValueTask<Result<TResponse>> HandleAsync(
        IFeature<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse()
            .Aggregate((RequestHandlerDelegate<TResponse>)Handler,
                (next, pipeline) => () => pipeline.HandleAsync((TRequest)request, next, cancellationToken))();

        ValueTask<Result<TResponse>> Handler()
        {
            return serviceProvider.GetRequiredService<IHandler<TRequest, TResponse>>()
                .HandleAsync((TRequest)request, cancellationToken);
        }
    }
}
