using System.Dynamic;
using LanguageExt;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;
using VSlices.Core;
using VSlices.CrossCutting.Pipeline.EventFiltering.MessageTemplates;
using VSlices.Domain.Interfaces;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Pipeline.EventFiltering;

/// <summary>
/// A filtering behavior using a custom logic
/// </summary>
/// <typeparam name="TRequest">The intercepted request used tofilter</typeparam>
/// <typeparam name="THandler">The intercepted handler, who might be skipped</typeparam>
public sealed class EventFilteringBehavior<TRequest, THandler>(
    IEventFilter<TRequest, THandler> eventFilter,
    IEventFilteringMessageTemplate template,
    ILogger<TRequest> logger,
    TimeProvider timeProvider)
    : IPipelineBehavior<TRequest, Unit>
    where TRequest : IEvent
    where THandler : IHandler<TRequest>
{
    private readonly IEventFilter<TRequest, THandler> _eventFilter = eventFilter;
    private readonly IEventFilteringMessageTemplate _template = template;
    private readonly ILogger<TRequest> _logger = logger;
    readonly TimeProvider _timeProvider = timeProvider;

    /// <inheritdoc />
    public Aff<Runtime, Unit> Define(TRequest request, Aff<Runtime, Unit> next) =>
        from _ in _eventFilter.Define(request)
            .Bind(c =>
            {
                if (c is false)
                {
                    _logger.LogInformation(_template.SkipExecution, 
                        _timeProvider.GetUtcNow(), typeof(THandler).FullName, request);

                    return unitAff;
                }

                _logger.LogInformation(_template.ContinueExecution,
                    _timeProvider.GetUtcNow(), typeof(THandler).FullName, request);

                return next;
            })
        select unit;
}

public interface IEventFilter;

public interface IEventFilter<in TEvent, THandler> : IEventFilter
    where TEvent : IEvent
    where THandler : IHandler<TEvent>
{
    /// <summary>
    /// Defines a filtering behavior for an <see cref="IEvent"/>
    /// </summary>
    /// <remarks>Return <value>true</value> if the execution continues, return <value>false</value> if not</remarks>
    /// <param name="event"></param>
    /// <returns></returns>
    Aff<Runtime, bool> Define(TEvent @event);
}