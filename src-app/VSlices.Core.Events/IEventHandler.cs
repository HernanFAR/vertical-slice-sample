using LanguageExt;
using VSlices.Base;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events;

/// <summary>
/// Defines asynchronous effect for a specific <see cref="IEvent"/>
/// </summary>
/// <remarks>If idempotency is necessary, the handler itself must ensure it</remarks>
/// <typeparam name="TEvent">The event to handle</typeparam>
public interface IEventHandler<in TEvent> : IHandler<TEvent, Unit>
    where TEvent : IEvent;
