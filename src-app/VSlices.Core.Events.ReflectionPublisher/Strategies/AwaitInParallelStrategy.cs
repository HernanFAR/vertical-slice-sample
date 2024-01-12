using VSlices.CrossCutting;

namespace VSlices.Core.Events.Strategies;

/// <summary>
/// A publishing strategy that awaits all handlers in parallel.
/// </summary>
public class AwaitInParallelStrategy : IPublishingStrategy
{
    /// <summary>
    /// Handles the given handlers in parallel using <see cref="Task.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>.
    /// </summary>
    /// <typeparam name="TResponse">Expected response</typeparam>
    /// <param name="handlerDelegates">Request Handlers</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async ValueTask HandleAsync<TResponse>(RequestHandlerDelegate<TResponse>[] handlerDelegates)
    {
        await Task.WhenAll(handlerDelegates.Select(async handlerDelegate => await handlerDelegate()));
    }
