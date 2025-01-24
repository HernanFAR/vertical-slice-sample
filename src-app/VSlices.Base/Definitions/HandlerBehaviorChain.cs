namespace VSlices.Base.Definitions;

internal abstract class HandlerBehaviorChain(IEnumerable<Type> behaviors)
{
    public IEnumerable<Type> Behaviors { get; } = behaviors;
}

internal sealed class BehaviorInterceptorChain<THandler>(IEnumerable<Type> behaviors)
    : HandlerBehaviorChain(behaviors);
