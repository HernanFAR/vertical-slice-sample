using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base.CrossCutting;
using VSlices.Base.Definitions;
using VSlices.CrossCutting.Interceptor.Logging;
using VSlices.CrossCutting.Interceptor.Logging.MessageTemplates;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="InterceptorChain"/> extensions for <see cref="LoggingInterceptor{TIn,TOut}"/>
/// </summary>
public static class LoggingBehaviorExtensions
{
    /// <summary>
    /// Adds a logging behavior in the pipeline execution related to this specific <see cref="IBehaviorInterceptor{TIn,TOut}"/>>
    /// </summary>
    public static LoggingBehaviorBuilder AddLogging(this InterceptorChain handlerEffects)
    {
        handlerEffects.Add(typeof(LoggingInterceptor<,>))
                 .Services.TryAddSingleton(TimeProvider.System);

        return new LoggingBehaviorBuilder(handlerEffects);
    }
}

/// <summary>
/// Builder for <see cref="LoggingInterceptor{TIn,TOut}"/>
/// </summary>
/// <param name="definition"></param>
public sealed class LoggingBehaviorBuilder(InterceptorChain definition)
{
    private readonly InterceptorChain _definition = definition;

    /// <summary>
    /// Add a custom <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public InterceptorChain In<TMessageTemplate>()
        where TMessageTemplate : class, ILoggingMessageTemplate
    {
        _definition.Services.AddSingleton<ILoggingMessageTemplate, TMessageTemplate>();

        return _definition;
    }

    /// <summary>
    /// Add an english <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public InterceptorChain InEnglish() => In<EnglishLoggingMessageTemplate>();

    /// <summary>
    /// Add a spanish <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public InterceptorChain InSpanish() => In<SpanishLoggingMessageTemplate>();

}
