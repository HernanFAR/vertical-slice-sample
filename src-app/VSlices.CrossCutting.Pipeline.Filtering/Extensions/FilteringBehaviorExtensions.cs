using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base.Core;
using VSlices.Base.Definitions;
using VSlices.CrossCutting.Interceptor.Filtering;
using VSlices.CrossCutting.Interceptor.Filtering.MessageTemplates;
using VSlices.Domain.Interfaces;

// ReSharper disable once CheckNamespace
namespace VSlices.Base.Builder;

/// <summary>
/// <see cref="InterceptorChain{TIn,TOut, TBehavior}"/> extensions for <see cref="FilteringBehaviorInterceptor{TRequest,TFilter,THandler}"/>
/// </summary>
public static class EventFilteringInterceptorExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="handlerEffects">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static EventFilteringBehaviorFilterBuilder<TIn, TBehavior> AddFiltering<TIn, TBehavior>(
        this InterceptorChain<TIn, Unit, TBehavior> handlerEffects)
        where TIn : IEvent
        where TBehavior : IBehavior<TIn> => 
        new(handlerEffects);
}

/// <summary>
/// Builder for <see cref="FilteringBehaviorInterceptor{TRequest,TFilter,THandler}"/>
/// </summary>
public sealed class EventFilteringBehaviorFilterBuilder<TIn, TBehavior>(InterceptorChain<TIn, Unit, TBehavior> handlerEffects)
    where TIn : IEvent
    where TBehavior : IBehavior<TIn>
{
    /// <summary>
    /// Specifies the filtering implementation to use
    /// </summary>
    /// <typeparam name="T">Filtering implementation</typeparam>
    /// <returns>Language builder for more configurations</returns>
    public EventFilteringBehaviorLanguageBuilder<TIn, TBehavior> Using<T>()
        where T : IEventFilter<TIn, TBehavior>
    {
        Type eventFilterType = typeof(T);

        handlerEffects.AddConcrete(typeof(FilteringBehaviorInterceptor<TIn, T, TBehavior>))
                       .Services.AddTransient(eventFilterType)
                       .TryAddSingleton(TimeProvider.System);

        return new EventFilteringBehaviorLanguageBuilder<TIn, TBehavior>(handlerEffects);
    }
}

/// <summary>
/// Language builder for <see cref="FilteringBehaviorInterceptor{TRequest,TFilter,THandler}"/>
/// </summary>
public sealed class EventFilteringBehaviorLanguageBuilder<TIn, TBehavior>(
    InterceptorChain<TIn, Unit, TBehavior> handlerEffects)
    where TBehavior : IBehavior<TIn, Unit>
{
    /// <summary>
    /// Add a custom <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public InterceptorChain<TIn, Unit, TBehavior> In<TMessageTemplate>()
        where TMessageTemplate : class, IEventFilteringMessageTemplate
    {
        handlerEffects.Services.AddSingleton<IEventFilteringMessageTemplate, TMessageTemplate>();

        return handlerEffects;
    }

    /// <summary>
    /// Add an english <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public InterceptorChain<TIn, Unit, TBehavior> InEnglish()
    {
        handlerEffects.Services.AddSingleton<IEventFilteringMessageTemplate, EnglishEventFilteringMessageTemplate>();

        return handlerEffects;
    }

    /// <summary>
    /// Add a spanish <see cref="IEventFilteringMessageTemplate"/>
    /// </summary>
    public InterceptorChain<TIn, Unit, TBehavior> InSpanish()
    {
        handlerEffects.Services.AddSingleton<IEventFilteringMessageTemplate, SpanishEventFilteringMessageTemplate>();

        return handlerEffects;
    }
}