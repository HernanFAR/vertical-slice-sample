using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace VSlices.Core.Events.Strategies;

/// <summary>
/// A publishing strategy that awaits each handler in sequence.
/// </summary>
public sealed class AwaitForEachStrategy : IPublishingStrategy
{
    /// <summary>
    /// Handles the given handlers in parallel using for each.
    /// </summary>
    public Fin<Unit> Handle(Eff<HandlerRuntime, Unit>[] delegates, HandlerRuntime runtime, CancellationToken cancellationToken)
    {
        List<Error> errors = new(delegates.Length);

        foreach (Eff<HandlerRuntime, Unit> handlerDelegate in delegates)
        {
            Fin<Unit> result = handlerDelegate.Run(runtime, cancellationToken);

            result.IfFail(errors.Add);
        }

        return errors.Count == 0 ? unit : Error.Many(errors.ToArray());
    }
}
