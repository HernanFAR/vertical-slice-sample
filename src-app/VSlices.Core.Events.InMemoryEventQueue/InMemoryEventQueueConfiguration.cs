namespace VSlices.Core.Events.InMemoryEventQueue;

/// <summary>
/// Configuration for <see cref="InMemoryEventQueue"/>
/// </summary>
public class InMemoryEventQueueConfiguration
{
    /// <summary>
    /// Capacity of the queue
    /// </summary>
    /// <remarks>Defaults to 50</remarks>
    public int Capacity { get; set; } = 50;
}
