using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Base.Definitions;
using VSlices.CrossCutting.Interceptor.Logging;
using VSlices.CrossCutting.Interceptor.Logging.MessageTemplates;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="InterceptorChain{TIn, TOut, TBehavior}"/> extensions for <see cref="LoggingInterceptor{TIn,TOut}"/>
/// </summary>
public static class LoggingBehaviorExtensions
{
    /// <summary>
    /// Adds a logging behavior in the pipeline execution related to this specific <see cref="IBehaviorInterceptor{TIn,TOut}"/>>
    /// </summary>
    public static LoggingBehaviorBuilder<TIn, TOut, TBehavior> AddLogging<TIn, TOut, TBehavior>(
        this InterceptorChain<TIn, TOut, TBehavior> handlerEffects)
        where TBehavior : IBehavior<TIn, TOut>
    {
        handlerEffects.Add(typeof(LoggingInterceptor<,>))
                 .Services.TryAddSingleton(TimeProvider.System);

        return new LoggingBehaviorBuilder<TIn, TOut, TBehavior>(handlerEffects);
    }
}

/// <summary>
/// Builder for <see cref="LoggingInterceptor{TIn,TOut}"/>
/// </summary>
/// <param name="definition"></param>
public sealed class LoggingBehaviorBuilder<TIn, TOut, TBehavior>(InterceptorChain<TIn, TOut, TBehavior> definition)
    where TBehavior : IBehavior<TIn, TOut>
{
    private readonly InterceptorChain<TIn, TOut, TBehavior> _definition = definition;

    /// <summary>
    /// Add a custom <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public InterceptorChain<TIn, TOut, TBehavior> In<TMessageTemplate>()
        where TMessageTemplate : class, ILoggingMessageTemplate
    {
        _definition.Services.AddSingleton<ILoggingMessageTemplate, TMessageTemplate>();

        return _definition;
    }

    /// <summary>
    /// Add an english <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public InterceptorChain<TIn, TOut, TBehavior> InEnglish() => In<EnglishLoggingMessageTemplate>();

    /// <summary>
    /// Add a spanish <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public InterceptorChain<TIn, TOut, TBehavior> InSpanish() => In<SpanishLoggingMessageTemplate>();

}
