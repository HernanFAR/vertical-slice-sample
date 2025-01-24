using LanguageExt;
using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.CrossCutting.Interceptor.Filtering.MessageTemplates;
using VSlices.Domain.Interfaces;
using static LanguageExt.Prelude;
using static VSlices.VSlicesPrelude;

namespace VSlices.CrossCutting.Interceptor.Filtering;

/// <summary>
/// A filtering behavior using a custom logic
/// </summary>
public sealed class FilteringBehaviorInterceptor<TRequest, TFilter, THandler> : IBehaviorInterceptor<TRequest, Unit>
    where TRequest : IEvent
    where TFilter : IEventFilter<TRequest, THandler>
    where THandler : IBehavior<TRequest>
{
    /// <inheritdoc />
    public Eff<VSlicesRuntime, Unit> Define(TRequest request, Eff<VSlicesRuntime, Unit> next) =>
        from eventFilter in provide<TFilter>()
        from template in provide<IEventFilteringMessageTemplate>()
        from logger in provide<ILogger<TRequest>>()
        from timeProvider in provide<TimeProvider>()
        from _ in eventFilter.DefineFilter(request)
                .Bind(c =>
                {
                    if (c is false)
                    {
                        logger.LogWarning(template.SkipExecution,
                            timeProvider.GetUtcNow(), typeof(TRequest).FullName, request);

                        return unitEff;
                    }

                    logger.LogInformation(template.ContinueExecution,
                        timeProvider.GetUtcNow(), typeof(TRequest).FullName, request);

                    return next;
                })
        select unit;
}

/// <summary>
/// Not intended to be used in development <see cref="IEventFilter{TEvent, THandler}"/>
/// </summary>
public interface IEventFilter;

/// <summary>
/// Not indented to be used in development, use <see cref="IEventFilter{TEvent, THandler}"/>
/// </summary>
public interface IEventFilter<in TEvent> : IEventFilter
    where TEvent : IEvent
{
    /// <summary>
    /// Defines a filtering behavior for an <see cref="IEvent"/>
    /// </summary>
    /// <remarks>Return <value>true</value> if the execution continues, return <value>false</value> if not</remarks>
    /// <param name="feature"></param>
    /// <returns></returns>
    Eff<VSlicesRuntime, bool> DefineFilter(TEvent feature);
}

/// <summary>
/// Defines a filtering behavior for an <see cref="IEvent"/>
/// </summary>
public interface IEventFilter<in TEvent, THandler> : IEventFilter<TEvent>
    where TEvent : IEvent
    where THandler : IBehavior<TEvent>;
