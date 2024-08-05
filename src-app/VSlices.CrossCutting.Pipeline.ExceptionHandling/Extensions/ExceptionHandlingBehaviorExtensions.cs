using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base.Builder;
using VSlices.CrossCutting.Pipeline.ExceptionHandling;
using VSlices.CrossCutting.Pipeline.ExceptionHandling.MessageTemplates;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureDefinition{TFeature,TResult}"/> extensions for <see cref="ExceptionHandlingBehavior{TRequest,TResult}"/>
/// </summary>
public static class ExceptionHandlingBehaviorExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="handlerEffects">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static ExceptionBehaviorBuilder AddLoggingException(this BehaviorChain handlerEffects)
    {
        handlerEffects.Add(typeof(LoggingExceptionBehavior<,>))
                         .Services.TryAddSingleton(TimeProvider.System);

        return new ExceptionBehaviorBuilder(handlerEffects);

    }
}

/// <summary>
/// Builder for <see cref="LoggingExceptionBehavior{TRequest,TResult}"/>
/// </summary>
/// <param name="definition"></param>
public sealed class ExceptionBehaviorBuilder(BehaviorChain definition)
{
    private readonly BehaviorChain _definition = definition;

    /// <summary>
    /// Add a custom <see cref="IExceptionMessageTemplate"/>
    /// </summary>
    public BehaviorChain UsingTemplate<TMessageTemplate>()
        where TMessageTemplate : class, IExceptionMessageTemplate
    {
        _definition.Services.AddSingleton<IExceptionMessageTemplate, TMessageTemplate>();

        return _definition;
    }

    /// <summary>
    /// Add an english <see cref="IExceptionMessageTemplate"/>
    /// </summary>
    public BehaviorChain UsingEnglish() => UsingTemplate<EnglishExceptionMessageTemplate>();

    /// <summary>
    /// Add a spanish <see cref="IExceptionMessageTemplate"/>
    /// </summary>
    public BehaviorChain UsingSpanish() => UsingTemplate<SpanishExceptionMessageTemplate>();

}