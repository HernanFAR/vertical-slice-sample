using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
using static LanguageExt.Prelude;

namespace VSlices.Core.Events.Strategies;

/// <summary>
/// A publishing strategy that awaits all handlers in parallel.
/// </summary>
public class AwaitInParallelStrategy : IPublishingStrategy
{
    /// <summary>
    /// Handles the given handlers in parallel using <see cref="Task.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>.
    /// </summary>
    /// <param name="handlerDelegates">Request Handlers</param>
    /// <param name="runtime">Execution runtime</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async ValueTask<Fin<Unit>> HandleAsync(Aff<Runtime, Unit>[] handlerDelegates, Runtime runtime)
    {
        IEnumerable<Task<Fin<Unit>>> tasks = handlerDelegates
            .Select(async handlerDelegate => await handlerDelegate.Run(runtime));

        Fin<Unit>[] results = await Task.WhenAll(tasks);

        List<Error> errors = new(results.Length);

        foreach (Fin<Unit> result in results)
        {
            result.IfFail(errors.Add);
        }

        return errors.Any() ? Error.Many(errors.ToArray()) : unit;
    }
}
