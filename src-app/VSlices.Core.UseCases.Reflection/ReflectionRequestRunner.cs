﻿using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using VSlices.Core.Traits;
using VSlices.Core.UseCases.Internals;

namespace VSlices.Core.UseCases;

/// <summary>
/// Sends a request through the VSlices pipeline to handle by a single handler, using reflection.
/// </summary>
[RequiresDynamicCode("This class uses Type.MakeGenericType to create RequestRunnerWrapper instances")]
public class ReflectionRequestRunner(IServiceProvider serviceProvider) : IRequestRunner
{
    private static readonly ConcurrentDictionary<Type, AbstractRequestRunnerWrapper> RequestHandlers = new();

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public Fin<TResponse> Run<TResponse>(IRequest<TResponse> request, HandlerRuntime runtime)
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

        return handler.Handle(request, runtime, _serviceProvider);
    }

    /// <inheritdoc />
    public Fin<TResult> Run<TResult>(IRequest<TResult> request,
                                     CancellationToken cancellationToken = default)
    {
        DependencyProvider dependencyProvider = new(_serviceProvider);
        var                envIo              = EnvIO.New(token: cancellationToken);

        return Run(request, HandlerRuntime.New(dependencyProvider, envIo));
    }
}
