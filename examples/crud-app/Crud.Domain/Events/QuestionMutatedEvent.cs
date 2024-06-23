using VSlices.Domain;

namespace Crud.Domain.Events;

public enum EState
{
    Created,
    Updated,
    Removed
}

public sealed class QuestionMutatedEvent : Event
{
    internal QuestionMutatedEvent(QuestionId id, EState currentState)
    {
        Id = id;
        CurrentState = currentState;
    }

    public QuestionId Id { get; }
    public EState CurrentState { get; }
}
