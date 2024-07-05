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
/// <see cref="FeatureBuilder"/> extensions for <see cref="EventFilteringBehavior{TRequest, THandler}"/>
/// </summary>
public static class EventFilteringBehaviorExtensions
{
    /// <summary>
    /// Adds an event filtering behavior using a specific <see cref="IEventFilter{TEvent, THandler}"/>
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static EventFilteringBehaviorBuilder AddEventFilteringUsing<T>(this FeatureBuilder featureBuilder)
        where T : IEventFilter
        => featureBuilder.AddEventFilteringUsing(typeof(T));

    /// <summary>
    /// Adds
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <param name="eventFilterImplementationType">Behavior</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static EventFilteringBehaviorBuilder AddEventFilteringUsing(this FeatureBuilder featureBuilder, Type eventFilterImplementationType)
    {
        Type eventFilterInterfaceType = eventFilterImplementationType
                                 .GetInterfaces()
                                 .Where(x => x.IsGenericType)
                                 .SingleOrDefault(x => x.GetGenericTypeDefinition() == typeof(IEventFilter<,>))
                             ?? throw new InvalidOperationException(
                                 $"{eventFilterImplementationType.FullName} does not implement {typeof(IEventFilter<,>).FullName}");

        Type eventType = eventFilterInterfaceType.GetGenericArguments()[0];
        Type handlerType = eventFilterInterfaceType.GetGenericArguments()[1];

        featureBuilder.Services.AddTransient(eventFilterInterfaceType, eventFilterImplementationType);

        Type pipelineBehaviorType = typeof(IPipelineBehavior<,>)
            .MakeGenericType(eventType, typeof(Unit));

        Type fluentValidationBehaviorType = typeof(EventFilteringBehavior<,>)
            .MakeGenericType(eventType, handlerType);

        featureBuilder.Services.AddTransient(pipelineBehaviorType, fluentValidationBehaviorType);

        return new EventFilteringBehaviorBuilder(featureBuilder);

    }
}

/// <summary>
/// Builder for <see cref="EventFilteringBehavior{TRequest, THandler}"/>
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
