using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.CrossCutting.Pipeline.Logging;
using VSlices.CrossCutting.Pipeline.Logging.MessageTemplates;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureDefinition{TFeature,TResult}"/> extensions for <see cref="LoggingBehavior{TRequest,TResult}"/>
/// </summary>
public static class LoggingBehaviorExtensions
{
    /// <summary>
    /// Adds a logging behavior in the pipeline execution related to this specific <see cref="IHandler{TFeature,TResult}"/>>
    /// </summary>
    public static LoggingBehaviorBuilder AddLogging(this BehaviorChain handlerEffects)
    {
        handlerEffects.Add(typeof(LoggingBehavior<,>))
                 .Services.TryAddSingleton(TimeProvider.System);

        return new LoggingBehaviorBuilder(handlerEffects);
    }
}

/// <summary>
/// Builder for <see cref="LoggingBehavior{TRequest,TResult}"/>
/// </summary>
/// <param name="definition"></param>
public sealed class LoggingBehaviorBuilder(BehaviorChain definition)
{
    private readonly BehaviorChain _definition = definition;

    /// <summary>
    /// Add a custom <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public BehaviorChain UsingTemplate<TMessageTemplate>()
        where TMessageTemplate : class, ILoggingMessageTemplate
    {
        _definition.Services.AddSingleton<ILoggingMessageTemplate, TMessageTemplate>();

        return _definition;
    }

    /// <summary>
    /// Add an english <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public BehaviorChain UsingEnglish() => UsingTemplate<EnglishLoggingMessageTemplate>();

    /// <summary>
    /// Add a spanish <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public BehaviorChain UsingSpanish() => UsingTemplate<SpanishLoggingMessageTemplate>();

}
