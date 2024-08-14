using LanguageExt;
using VSlices.Base;
using VSlices.Base.Core;

namespace VSlices.Core.Events.Strategies;

/// <summary>
/// Defines a publishing strategy for the <see cref="IEventRunner"/>.
/// </summary>
public interface IPublishingStrategy
{
    /// <summary>
    /// Handles the execution of the <see cref="IHandler{TFeature}"/>'s related
    /// to the event.
    /// </summary>
    Fin<Unit> Handle(Eff<VSlicesRuntime, Unit>[] delegates, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}
