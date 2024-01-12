using VSlices.CrossCutting;

namespace VSlices.Core.Events.ReflectionPublisher.Strategies;

/// <summary>
/// A publishing strategy that awaits each handler in sequence.
/// </summary>
public class AwaitForEachStrategy : IPublishingStrategy
{
    /// <summary>
    /// Handles the given handlers in parallel using for each.
    /// </summary>
    /// <typeparam name="TResponse">Expected response</typeparam>
    /// <param name="handlerDelegates">Request Handlers</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async ValueTask HandleAsync<TResponse>(RequestHandlerDelegate<TResponse>[] handlerDelegates)
    {
        foreach (var handlerDelegate in handlerDelegates)
        {
            _ = await handlerDelegate();
        }
    }
}
