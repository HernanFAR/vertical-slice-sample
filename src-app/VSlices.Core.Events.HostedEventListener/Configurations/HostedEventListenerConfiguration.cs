namespace VSlices.Core.Events.Configurations;

/// <summary>
/// Represents what to do in the background event listener when a problem occurs in the event's <see cref="IHandler{TRequest}"/>
/// </summary>
public enum MoveActions
{
    /// <summary>
    /// Move to the last position in the queue
    /// </summary>
    MoveLast,
    /// <summary>
    /// Immediate retry the event <see cref="IHandler{TRequest}"/>
    /// </summary>
    ImmediateRetry
}

/// <summary>
/// Represents the configuration for the background event listener
/// </summary>
public class HostedEventListenerConfiguration
{
    /// <summary>
    /// Represents what to do then an exception occurs in the event's <see cref="IHandler{TRequest}"/>
    /// </summary>
    public MoveActions ActionInException { get; set; } = MoveActions.MoveLast;

    /// <summary>
    /// Represents the maximum number of retries for the event's <see cref="IHandler{TRequest}"/>
    /// </summary>
    public int MaxRetries { get; set; } = 3;

}
