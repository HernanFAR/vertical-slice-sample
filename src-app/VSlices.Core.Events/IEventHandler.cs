using LanguageExt;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events;

public interface IEventHandler<in TEvent, THandler>
    where TEvent : IEvent
    where THandler : IEventHandler<TEvent, THandler>
{

}
