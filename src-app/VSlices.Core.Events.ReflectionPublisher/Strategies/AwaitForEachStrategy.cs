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
    /// <param name="delegates">Request Handlers</param>
    /// <param name="runtime">Execution runtime</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Fin<Unit> Handle(Eff<HandlerRuntime, Unit>[] delegates, HandlerRuntime runtime)
    {
        List<Error> errors = new(delegates.Length);

        foreach (Eff<HandlerRuntime, Unit> handlerDelegate in delegates)
        {
            Fin<Unit> result = handlerDelegate.Run(runtime, runtime.EnvIO);

            result.IfFail(errors.Add);
        }

        return errors.Count == 0 ? Error.Many(errors.ToArray()) : unit;
    }
}
