using System.Collections.Concurrent;
using VSlices.Base.Responses;
using VSlices.Core.UseCases.Reflection.Internals;

namespace VSlices.Core.UseCases.Reflection;

/// <summary>
/// Sends a request through the VSlices pipeline to be handled by a single handler, using reflection
/// </summary>
public class ReflectionSender : ISender
{
    private static readonly ConcurrentDictionary<Type, AbstractHandlerWrapper> RequestHandlers = new();

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates a new instance of <see cref="ReflectionSender"/>
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/> used to resolve handlers</param>
    public ReflectionSender(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async ValueTask<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handler = (AbstractHandlerWrapper<TResponse>)RequestHandlers.GetOrAdd(request.GetType(), static requestType =>
        {
            var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
            var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
            return (AbstractHandlerWrapper)wrapper;
        });

        return await handler.HandleAsync(request, _serviceProvider, cancellationToken);
    }
}
