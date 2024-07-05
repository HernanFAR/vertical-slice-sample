using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base;
using VSlices.CrossCutting.Pipeline;
using VSlices.CrossCutting.Pipeline.EventFiltering;
using VSlices.CrossCutting.Pipeline.EventFiltering.MessageTemplates;
using VSlices.Domain;
using VSlices.Domain.Interfaces;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureBuilder"/> extensions for <see cref="EventFilteringBehavior{TRequest}"/>
/// </summary>
public static class EventFilteringBehaviorExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static EventFilteringBehaviorBuilder AddEventFilteringUsing<T>(this FeatureBuilder featureBuilder)
        where T : IEventFilter
        => featureBuilder.AddEventFilteringUsing(typeof(T));

    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <param name="eventFilterType">Behavior</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static EventFilteringBehaviorBuilder AddEventFilteringUsing(this FeatureBuilder featureBuilder, Type eventFilterType)
    {
        Type validatorType = eventFilterType
                                 .GetInterfaces()
                                 .Where(x => x.IsGenericType)
                                 .SingleOrDefault(x => x.GetGenericTypeDefinition() == typeof(IEventFilter<>))
                             ?? throw new InvalidOperationException(
                                 $"{eventFilterType.FullName} does not implement {typeof(IEventFilter<>).FullName}");

        Type eventType = validatorType.GetGenericArguments()[0];

        featureBuilder.Services.AddTransient(validatorType, eventFilterType);

        Type pipelineBehaviorType = typeof(IPipelineBehavior<,>)
            .MakeGenericType(eventType, typeof(Unit));

        Type fluentValidationBehaviorType = typeof(EventFilteringBehavior<>)
            .MakeGenericType(eventType);

        featureBuilder.Services.AddTransient(pipelineBehaviorType, fluentValidationBehaviorType);

        return new EventFilteringBehaviorBuilder(featureBuilder);

    }
}

/// <summary>
/// Builder for <see cref="EventFilteringBehavior{TRequest}"/>
/// </summary>
/// <param name="builder"></param>
public sealed class EventFilteringBehaviorBuilder(FeatureBuilder builder)
{
    readonly FeatureBuilder _builder = builder;

    /// <summary>
    /// Add a custom <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public FeatureBuilder UsingTemplate<TMessageTemplate>()
        where TMessageTemplate : class, IEventFilteringMessageTemplate
    {
        _builder.Services.AddSingleton<IEventFilteringMessageTemplate, TMessageTemplate>();

        return _builder;
    }

    /// <summary>
    /// Add a english <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public FeatureBuilder UsingEnglishTemplate()
    {
        _builder.Services.AddSingleton<IEventFilteringMessageTemplate, EnglishEventFilteringMessageTemplate>();

        return _builder;
    }

    /// <summary>
    /// Add a spanish <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public FeatureBuilder UsingSpanishTemplate()
    {
        _builder.Services.AddSingleton<IEventFilteringMessageTemplate, SpanishEventFilteringMessageTemplate>();

        return _builder;
    }
}
