using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base.Core;
using VSlices.Base.Definitions;
using VSlices.CrossCutting.Interceptor.ExceptionHandling;
using VSlices.CrossCutting.Interceptor.ExceptionHandling.MessageTemplates;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="InterceptorChain"/> extensions for <see cref="ExceptionHandlingInterceptor{TIn,TOut}"/>
/// </summary>
public static class ExceptionHandlingBehaviorExtensions
{
    /// <summary>
    /// Adds a LoggingExceptionInterceptor to the <see cref="InterceptorChain"/> of the associated <see cref="IBehavior{TIn, TOut}"/>
    /// </summary>
    /// <param name="handlerEffects">The interceptor chain of the associated <see cref="IBehavior{TIn,TOut}"/></param>
    /// <returns>Builder for more specific configuration</returns>
    public static ExceptionBehaviorBuilder AddLoggingException(this InterceptorChain handlerEffects)
    {
        handlerEffects.Add(typeof(LoggingExceptionInterceptor<,>))
                         .Services.TryAddSingleton(TimeProvider.System);

        return new ExceptionBehaviorBuilder(handlerEffects);

    }
}

/// <summary>
/// Builder for <see cref="LoggingExceptionInterceptor{TIn,TOut}"/>
/// </summary>
/// <param name="definition">Interceptor chain of the associated <see cref="IBehavior{TIn,TOut}"/></param>
public sealed class ExceptionBehaviorBuilder(InterceptorChain definition)
{
    private readonly InterceptorChain _definition = definition;

    /// <summary>
    /// Add a custom <see cref="IExceptionMessageTemplate"/>
    /// </summary>
    public InterceptorChain In<TMessageTemplate>()
        where TMessageTemplate : class, IExceptionMessageTemplate
    {
        _definition.Services.AddSingleton<IExceptionMessageTemplate, TMessageTemplate>();

        return _definition;
    }

    /// <summary>
    /// Add an english <see cref="IExceptionMessageTemplate"/>
    /// </summary>
    public InterceptorChain InEnglish() => In<EnglishExceptionMessageTemplate>();

    /// <summary>
    /// Add a spanish <see cref="IExceptionMessageTemplate"/>
    /// </summary>
    public InterceptorChain InSpanish() => In<SpanishExceptionMessageTemplate>();

}