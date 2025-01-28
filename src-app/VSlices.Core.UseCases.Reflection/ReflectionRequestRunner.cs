using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.UseCases.Internals;

namespace VSlices.Core.UseCases;

/// <summary>
/// Sends a input through the VSlices pipeline to handle by a single handler, using reflection.
/// </summary>
[RequiresDynamicCode("This class uses Type.MakeGenericType to create RequestRunnerWrapper instances")]
public class ReflectionRequestRunner(IServiceProvider serviceProvider) : IRequestRunner
{
    private static readonly ConcurrentDictionary<Type, AbstractRequestRunnerWrapper> RequestHandlers = new();

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public Fin<TResponse> Run<TResponse>(IInput<TResponse> input, CancellationToken cancellationToken)
    {
        var handler = (AbstractRequestRunnerWrapper<TResponse>)RequestHandlers
            .GetOrAdd(input.GetType(), 
                requestType =>
                {
                    Type wrapperType = typeof(RequestRunnerWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
                    object wrapper = Activator.CreateInstance(wrapperType) 
                                     ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
                    
                    return (AbstractRequestRunnerWrapper)wrapper;
                });

        using IServiceScope scope = _serviceProvider.CreateScope();

        return handler.Handle(input, scope.ServiceProvider, cancellationToken);
    }
}
