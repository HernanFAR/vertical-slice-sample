using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base.Core;
using VSlices.Base.Definitions;
using VSlices.CrossCutting.Interceptor.ExceptionHandling;
using VSlices.CrossCutting.Interceptor.ExceptionHandling.MessageTemplates;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="InterceptorChain{TIn,TOut, TBehavior}"/> extensions for <see cref="ExceptionHandlingInterceptor{TIn,TOut}"/>
/// </summary>
public static class ExceptionHandlingInterceptorExtensions
{
    /// <summary>
    /// Adds a LoggingExceptionInterceptor to the <see cref="InterceptorChain{TIn,TOut, TBehavior}"/> of the associated <see cref="IBehavior{TIn, TOut}"/>
    /// </summary>
    /// <param name="handlerEffects">The interceptor chain of the associated <see cref="IBehavior{TIn,TOut}"/></param>
    /// <returns>Builder for more specific configuration</returns>
    public static ExceptionHandlingInterceptorImplementationBuilder<TIn, TOut, TBehavior> AddExceptionHandling<TIn, TOut, TBehavior>(
        this InterceptorChain<TIn, TOut, TBehavior> handlerEffects)
        where TBehavior : IBehavior<TIn, TOut> =>
        new(handlerEffects);
}

/// <summary>
/// Builder for <see cref="LoggingExceptionInterceptor{TIn,TOut}"/>
/// </summary>
/// <param name="handlerEffects">Interceptor chain of the associated <see cref="IBehavior{TIn,TOut}"/></param>
public sealed class ExceptionHandlingInterceptorImplementationBuilder<TIn, TOut, TBehavior>(InterceptorChain<TIn, TOut, TBehavior> handlerEffects)
    where TBehavior : IBehavior<TIn, TOut>
{
    /// <summary>
    /// Adds a <see cref="LoggingExceptionInterceptor{TIn,TOut}" /> to the specified handler
    /// </summary>
    /// <returns>Next step builder</returns>
    public ExceptionHandlingInterceptorLanguageBuilder<TIn, TOut, TBehavior> UsingLogging()
    {
        handlerEffects.Add(typeof(LoggingExceptionInterceptor<,>))
                      .Services.TryAddSingleton(TimeProvider.System);

        return new ExceptionHandlingInterceptorLanguageBuilder<TIn, TOut, TBehavior>(handlerEffects);
    }
}

/// <summary>
/// Builder for <see cref="LoggingExceptionInterceptor{TIn,TOut}"/>
/// </summary>
/// <param name="handlerEffects">Interceptor chain of the associated <see cref="IBehavior{TIn,TOut}"/></param>
public sealed class ExceptionHandlingInterceptorLanguageBuilder<TIn, TOut, TBehavior>(InterceptorChain<TIn, TOut, TBehavior> handlerEffects)
    where TBehavior : IBehavior<TIn, TOut>
{
    /// <summary>
    /// Add a custom <see cref="IExceptionMessageTemplate"/>
    /// </summary>
    public InterceptorChain<TIn, TOut, TBehavior> In<TMessageTemplate>()
        where TMessageTemplate : class, IExceptionMessageTemplate
    {
        handlerEffects.Services.AddSingleton<IExceptionMessageTemplate, TMessageTemplate>();

        return handlerEffects;
    }

    /// <summary>
    /// Add an english <see cref="IExceptionMessageTemplate"/>
    /// </summary>
    public InterceptorChain<TIn, TOut, TBehavior> InEnglish() => In<EnglishExceptionMessageTemplate>();

    /// <summary>
    /// Add a spanish <see cref="IExceptionMessageTemplate"/>
    /// </summary>
    public InterceptorChain<TIn, TOut, TBehavior> InSpanish() => In<SpanishExceptionMessageTemplate>();

}