using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.CrossCutting.Pipeline.Filtering;
using VSlices.CrossCutting.Pipeline.Filtering.MessageTemplates;

// ReSharper disable once CheckNamespace
namespace VSlices.Base.Builder;

/// <summary>
/// <see cref="BehaviorChain"/> extensions for <see cref="FilteringBehavior{TRequest, TFilter, THandler}"/>
/// </summary>
public static class EventFilteringBehaviorExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="handlerEffects">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static EventFilteringBehaviorBuilder AddFilteringUsing<T>(this BehaviorChain handlerEffects)
    {
        Type eventFilterType = typeof(T);

        Type behaviorType = typeof(FilteringBehavior<,,>)
            .MakeGenericType(handlerEffects.FeatureType, 
                             eventFilterType, 
                             handlerEffects.HandlerType);

        handlerEffects.AddConcrete(behaviorType)
                      .Services.AddTransient(eventFilterType)
                      .TryAddSingleton(TimeProvider.System);

        return new EventFilteringBehaviorBuilder(handlerEffects);
    }
}

/// <summary>
/// Builder for <see cref="FilteringBehavior{TRequest, TFilter, THandler}"/>
/// </summary>
public sealed class EventFilteringBehaviorBuilder(BehaviorChain builder)
{
    private readonly BehaviorChain _builder = builder;

    /// <summary>
    /// Add a custom <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public BehaviorChain In<TMessageTemplate>()
        where TMessageTemplate : class, IEventFilteringMessageTemplate
    {
        _builder.Services.AddSingleton<IEventFilteringMessageTemplate, TMessageTemplate>();

        return _builder;
    }

    /// <summary>
    /// Add an english <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public BehaviorChain InEnglish()
    {
        _builder.Services.AddSingleton<IEventFilteringMessageTemplate, EnglishEventFilteringMessageTemplate>();

        return _builder;
    }

    /// <summary>
    /// Add a spanish <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public BehaviorChain InSpanish()
    {
        _builder.Services.AddSingleton<IEventFilteringMessageTemplate, SpanishEventFilteringMessageTemplate>();

        return _builder;
    }
}