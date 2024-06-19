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
    /// <param name="handlerDelegates">Request Handlers</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async ValueTask<Fin<Unit>> HandleAsync(Aff<Unit>[] handlerDelegates)
    {
        List<Error> errors = new(handlerDelegates.Length);

        foreach (Aff<Unit> handlerDelegate in handlerDelegates)
        {
            Fin<Unit> result = await handlerDelegate.Run();

            result.IfFail(errors.Add);
        }

        return errors.Any() ? Error.Many(errors.ToArray()) : unit;
    }
}
