using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Core;
using VSlices.Base.Definitions;
using VSlices.CrossCutting.Interceptor.FluentValidation;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="InterceptorChain{TIn,TOut}"/> extensions for <see cref="FluentValidationInterceptor{TIn,TOut}"/>
/// </summary>
public static class FluentValidationBehaviorExtensions
{
    /// <summary>
    /// Adds a fluent validation behavior in the pipeline execution related to this specific handler.
    /// </summary>
    public static FluentValidationInterceptorBuilder<TIn, TOut, TBehavior> AddValidation<TIn, TOut, TBehavior>(
        this InterceptorChain<TIn, TOut, TBehavior> handlerEffects)
        where TBehavior : IBehavior<TIn, TOut> =>
        new(handlerEffects);
}

public sealed class FluentValidationInterceptorBuilder<TIn, TOut, TBehavior>(InterceptorChain<TIn, TOut, TBehavior> handlerEffects)
    where TBehavior : IBehavior<TIn, TOut>
{
    public InterceptorChain<TIn, TOut, TBehavior> UsingFluent<T>()
        where T : class, IValidator<TIn>
    {
        handlerEffects.Add(typeof(FluentValidationInterceptor<,>))
                      .Services.AddTransient<IValidator<TIn>, T>();

        return handlerEffects;
    }
}