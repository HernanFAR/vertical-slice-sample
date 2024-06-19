using Microsoft.Extensions.Hosting;

namespace VSlices.Core.Events;

/// <summary>
/// Listens to an event queue and publishes the event to an event pipeline
/// </summary>
/// <remarks>
/// A scope is created for each event to be published
/// </remarks>
public sealed class HostedEventListener : BackgroundService
{
    private readonly IEventListenerCore _eventListener;

    /// <summary>
    /// Initializes a new instance of the <see cref="HostedEventListener"/> class.
    /// </summary>
    /// <param name="eventListener">Event listener core</param>
    public HostedEventListener(IEventListenerCore eventListener)
    {
        _eventListener = eventListener;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _eventListener.ProcessEvents(stoppingToken);
    }
}
