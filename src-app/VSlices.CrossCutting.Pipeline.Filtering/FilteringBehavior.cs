using LanguageExt;
using VSlices.Base;
using VSlices.Base.Core;
using VSlices.Domain.Interfaces;

namespace VSlices.CrossCutting.Interceptor.Filtering;

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
