using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
using VSlices.Core.UseCases.Internals;

namespace VSlices.Core.UseCases;

/// <summary>
/// Sends a request through the VSlices pipeline to be handled by a single handler, using reflection
/// </summary>
[RequiresDynamicCode("This class uses Type.MakeGenericType to create RequestRunnerWrapper instances")]
public class ReflectionRequestRunner : IRequestRunner
{
    private static readonly ConcurrentDictionary<Type, AbstractRequestRunnerWrapper> RequestHandlers = new();

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates a new instance of <see cref="ReflectionRequestRunner"/>
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/> used to resolve handlers</param>
    public ReflectionRequestRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async ValueTask<Fin<TResponse>> RunAsync<TResponse>(IRequest<TResponse> request, Runtime runtime)
    {
        var handler = (AbstractRequestRunnerWrapper<TResponse>)RequestHandlers
            .GetOrAdd(request.GetType(), 
                requestType =>
                {
                    Type wrapperType = typeof(RequestRunnerWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
                    object wrapper = Activator.CreateInstance(wrapperType) 
                                     ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
                    
                    return (AbstractRequestRunnerWrapper)wrapper;
                });

        return await handler.HandleAsync(request, runtime, _serviceProvider);
    }

    /// <inheritdoc />
    public async ValueTask<Fin<TResult>> RunAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken)
    {
        using var source = new CancellationTokenSource();

        await using (cancellationToken.Register(source.Cancel))
        {
            return await RunAsync(request, Runtime.New(ActivityEnv.Default, source));
        }
    }
}
