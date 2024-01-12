namespace VSlices.Core.Events;

/// <summary>
/// Defines a listener for events
/// </summary>
public interface IEventListener
{
    /// <summary>
    /// Starts an event listener process
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous execution of the process</returns>
    Task ProcessEvents(CancellationToken  cancellationToken);
}
