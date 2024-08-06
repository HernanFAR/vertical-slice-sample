namespace VSlices.Base.Builder;

internal sealed class HandlerBehaviorChain<THandler>(IEnumerable<Type> behaviors)
{
    public IEnumerable<Type> Behaviors { get; } = behaviors;
}