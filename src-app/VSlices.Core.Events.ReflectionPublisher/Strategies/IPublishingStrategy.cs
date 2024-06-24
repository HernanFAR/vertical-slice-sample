using LanguageExt;
using LanguageExt.SysX.Live;

namespace VSlices.Core.Events.Strategies;

/// <summary>
/// Defines a publishing strategy for the <see cref="IEventRunner"/>.
/// </summary>
public interface IPublishingStrategy
{
    /// <summary>
    /// Handles the execution of the <see cref="IHandler{TRequest,TResponse}"/>'s related to the event
    /// </summary>
    /// <param name="handlerDelegates">Handlers related to the event</param>
    /// <param name="runtime">Runtime execution for the handler</param>
    /// <returns></returns>
    ValueTask<Fin<Unit>> HandleAsync(Aff<Runtime, Unit>[] handlerDelegates, Runtime runtime);
}
