using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
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
    /// <param name="handlerDelegates">Request Handlers</param>
    /// <param name="runtime">Execution runtime</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async ValueTask<Fin<Unit>> HandleAsync(Aff<Runtime, Unit>[] handlerDelegates, Runtime runtime)
    {
        List<Error> errors = new(handlerDelegates.Length);

        foreach (Aff<Runtime, Unit> handlerDelegate in handlerDelegates)
        {
            Fin<Unit> result = await handlerDelegate.Run(runtime);

            result.IfFail(errors.Add);
        }

        return errors.Any() ? Error.Many(errors.ToArray()) : unit;
    }
}
