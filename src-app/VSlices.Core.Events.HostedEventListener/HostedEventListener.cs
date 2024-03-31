using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VSlices.Core.Events.Configurations;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events;

/// <summary>
/// Listens to an event queue and publishes the event to an event pipeline
/// </summary>
/// <remarks>
/// A scope is created for each event to be published
/// </remarks>
public sealed class HostedEventListener : BackgroundService
{
    private readonly IEventListener _eventListener;

    public HostedEventListener(IEventListener eventListener)
    {
        _eventListener = eventListener;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _eventListener.ProcessEvents(stoppingToken);
    }
}
