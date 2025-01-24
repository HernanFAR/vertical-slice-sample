using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base.Definitions;
using VSlices.CrossCutting.Interceptor.Filtering;
using VSlices.CrossCutting.Interceptor.Filtering.MessageTemplates;

// ReSharper disable once CheckNamespace
namespace VSlices.Base.Builder;

/// <summary>
/// <see cref="InterceptorChain"/> extensions for <see cref="FilteringBehaviorInterceptorInterceptor{TRequest,TFilter,THandler}"/>
/// </summary>
public static class EventFilteringBehaviorExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="handlerEffects">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static EventFilteringBehaviorBuilder AddFilteringUsing<T>(this InterceptorChain handlerEffects)
    {
        Type eventFilterType = typeof(T);

        Type behaviorType = typeof(FilteringBehaviorInterceptor<,,>)
            .MakeGenericType(handlerEffects.InType, 
                             eventFilterType, 
                             handlerEffects.BehaviorType);

        handlerEffects.AddConcrete(behaviorType)
                      .Services.AddTransient(eventFilterType)
                      .TryAddSingleton(TimeProvider.System);

        return new EventFilteringBehaviorBuilder(handlerEffects);
    }
}

/// <summary>
/// Builder for <see cref="FilteringBehaviorInterceptorInterceptor{TRequest,TFilter,THandler}"/>
/// </summary>
public sealed class EventFilteringBehaviorBuilder(InterceptorChain builder)
{
    private readonly InterceptorChain _builder = builder;

    /// <summary>
    /// Add a custom <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public InterceptorChain In<TMessageTemplate>()
        where TMessageTemplate : class, IEventFilteringMessageTemplate
    {
        _builder.Services.AddSingleton<IEventFilteringMessageTemplate, TMessageTemplate>();

        return _builder;
    }

    /// <summary>
    /// Add an english <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public InterceptorChain InEnglish()
    {
        _builder.Services.AddSingleton<IEventFilteringMessageTemplate, EnglishEventFilteringMessageTemplate>();

        return _builder;
    }

    /// <summary>
    /// Add a spanish <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public InterceptorChain InSpanish()
    {
        _builder.Services.AddSingleton<IEventFilteringMessageTemplate, SpanishEventFilteringMessageTemplate>();

        return _builder;
    }
}