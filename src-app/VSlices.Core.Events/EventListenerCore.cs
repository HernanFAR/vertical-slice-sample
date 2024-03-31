using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VSlices.Core.Events.Configurations;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events;

/// <summary>
/// Default implementation of <see cref="IEventListenerCore" />
/// </summary>
/// <remarks>
/// It has a retry mecanism for each <see cref="IEvent" />
/// </remarks>
public sealed class EventListenerCore : IEventListenerCore
{
    private readonly ILogger<EventListenerCore> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<EventListenerConfiguration> _configOptions;
    private readonly IEventQueue _eventQueue;
    private readonly Dictionary<Guid, int> _retries = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EventListenerCore"/> class.
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="configOptions">Configuration</param>
    public EventListenerCore(ILogger<EventListenerCore> logger,
        IServiceProvider serviceProvider,
        IOptionsMonitor<EventListenerConfiguration> configOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configOptions = configOptions;
        _eventQueue = serviceProvider.GetRequiredService<IEventQueue>();
    }

    /// <inheritdoc />
    public async Task ProcessEvents(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

            var workItem = await _eventQueue.DequeueAsync(cancellationToken);
            var retry = false;

            do
            {
                try
                {
                    await publisher.PublishAsync(workItem, cancellationToken);

                    _retries.Remove(workItem.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing {WorkItem}.", workItem.GetType().FullName);

                    retry = await CheckRetry(workItem, cancellationToken);
                }
            }
            while (retry);
        }
    }

    private async Task<bool> CheckRetry(IEvent workItem, CancellationToken stoppingToken)
    {
        if (_retries.TryGetValue(workItem.Id, out var retries))
        {
            _retries[workItem.Id] = retries + 1;
        }
        else
        {
            _retries.Add(workItem.Id, 1);
        }

        if (_retries[workItem.Id] > _configOptions.CurrentValue.MaxRetries)
        {
            _logger.LogError("Max retries {RetryLimit} reached for {WorkItem}.",
                _configOptions.CurrentValue.MaxRetries, workItem);

            _retries.Remove(workItem.Id);

            return false;
        }

        switch (_configOptions.CurrentValue.ActionInException)
        {
            case MoveActions.MoveLast:
                await _eventQueue.EnqueueAsync(workItem, stoppingToken);

                return false;

            case MoveActions.ImmediateRetry:

                return true;

            default:
                throw new InvalidOperationException(nameof(_configOptions.CurrentValue.ActionInException));
        }
    }
}
