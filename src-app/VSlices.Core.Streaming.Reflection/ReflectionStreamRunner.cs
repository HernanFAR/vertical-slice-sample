using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using LanguageExt.SysX.Live;
using VSlices.Core.Stream.Internals;

namespace VSlices.Core.Stream;

/// <summary>
/// Sends a request through the VSlices pipeline to be handled by a single handler, using reflection
/// </summary>
[RequiresDynamicCode("This class uses Type.MakeGenericType to create StreamRunnerWrapper instances")]
public class ReflectionStreamRunner : IStreamRunner
{
    private static readonly ConcurrentDictionary<Type, AbstractStreamRunnerWrapper> RequestHandlers = new();

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates a new instance of <see cref="ReflectionStreamRunner"/>
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/> used to resolve handlers</param>
    public ReflectionStreamRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async ValueTask<Fin<IAsyncEnumerable<TResult>>> RunAsync<TResult>(IStream<TResult> request, Runtime runtime)
    {
        var handler = (AbstractStreamRunnerWrapper<TResult>)RequestHandlers
            .GetOrAdd(request.GetType(), 
                requestType =>
                {
                    Type wrapperType = typeof(StreamRunnerWrapper<,>).MakeGenericType(requestType, typeof(TResult));
                    object wrapper = Activator.CreateInstance(wrapperType) 
                                     ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
                    
                    return (AbstractStreamRunnerWrapper)wrapper;
                });

        return await handler.HandleAsync(request, runtime, _serviceProvider);
    }

    /// <inheritdoc />
    public async ValueTask<Fin<IAsyncEnumerable<TResult>>> RunAsync<TResult>(
        IStream<TResult> request, 
        CancellationToken cancellationToken)
    {
        using CancellationTokenSource source = new();

        await using (cancellationToken.Register(source.Cancel))
        {
            return await RunAsync(request, Runtime.New(ActivityEnv.Default, source));
        }
    }
}
