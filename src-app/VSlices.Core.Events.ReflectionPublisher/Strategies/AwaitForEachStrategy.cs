using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
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
    public Fin<Unit> Handle(Eff<VSlicesRuntime, Unit>[] delegates, 
                            IServiceProvider serviceProvider, 
                            CancellationToken cancellationToken)
    {
        List<Error> errors = new(delegates.Length);

        foreach (Eff<VSlicesRuntime, Unit> handlerDelegate in delegates)
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            var runtime = scope.ServiceProvider
                               .GetRequiredService<VSlicesRuntime>();

            Fin<Unit> result = handlerDelegate.Run(runtime, cancellationToken);

            result.IfFail(errors.Add);
        }

        return errors.Count == 0 ? unit : Error.Many(errors.ToArray());
    }
}
