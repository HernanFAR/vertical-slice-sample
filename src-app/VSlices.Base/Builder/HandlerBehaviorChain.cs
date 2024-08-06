namespace VSlices.Base.Builder;

internal abstract class HandlerBehaviorChain(IEnumerable<Type> behaviors)
{
    public IEnumerable<Type> Behaviors { get; } = behaviors;
}

internal sealed class HandlerBehaviorChain<THandler>(IEnumerable<Type> behaviors)
    : HandlerBehaviorChain(behaviors);