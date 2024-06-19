using LanguageExt;

namespace VSlices.Core.Events.Strategies;

/// <summary>
/// Defines a publishing strategy for the <see cref="IEventRunner"/>.
/// </summary>
public interface IPublishingStrategy
{
    /// <summary>
    /// Handles the execution of the <see cref="IHandler{TRequest,TResponse}"/>'s related to the event
    /// </summary>
    /// <param name="handlerDelegates"></param>
    /// <returns></returns>
    ValueTask<Fin<Unit>> HandleAsync(Aff<Unit>[] handlerDelegates);
}
