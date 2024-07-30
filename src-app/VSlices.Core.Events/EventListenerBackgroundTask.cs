using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSlices.Core.Events.Configurations;
using VSlices.CrossCutting.BackgroundTaskListener;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events;

/// <summary>
/// Core of event listener
/// </summary>
/// <remarks>
/// It has a retry mechanism for each <see cref="IEvent" />
/// </remarks>
public sealed class EventListenerBackgroundTask(
    ILogger<EventListenerBackgroundTask> logger,
    IServiceProvider serviceProvider,
    EventListenerConfiguration configOptions) 
    : IBackgroundTask
{
    private readonly ILogger<EventListenerBackgroundTask> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly EventListenerConfiguration _config = configOptions;
    private readonly IEventQueue _eventQueue = serviceProvider.GetRequiredService<IEventQueue>();

    private readonly Dictionary<Guid, int> _retries = [];

    /// <inheritdoc />
    public string Identifier => nameof(EventListenerBackgroundTask);

    /// <inheritdoc />
    public async ValueTask ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();

            var publisher = scope.ServiceProvider.GetRequiredService<IEventRunner>();

            IEvent workItem = await _eventQueue.DequeueAsync(cancellationToken);
            var retry = false;

            do
            {
                try
                {
                    Fin<Unit> result = publisher.Publish(workItem, cancellationToken);

                    _ = result.IfFail(error => throw error);

                    _retries.Remove(workItem.EventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing {WorkItem}.",
                        workItem.GetType().FullName);

                    retry = await CheckRetry(workItem, cancellationToken);
                }
            }
            while (retry);
        }
    }

    private async Task<bool> CheckRetry(IEvent workItem, CancellationToken stoppingToken)
    {
        if (_retries.TryGetValue(workItem.EventId, out int retries))
        {
            _retries[workItem.EventId] = retries + 1;
        }
        else
        {
            _retries.Add(workItem.EventId, 1);
        }

        if (_retries[workItem.EventId] > _config.MaxRetries)
        {
            _logger.LogError("Max retries {RetryLimit} reached for {WorkItem}.",
                _config.MaxRetries, workItem);

            _retries.Remove(workItem.EventId);

            return false;
        }

        switch (_config.ActionInException)
        {
            case MoveActions.MoveLast:
                await _eventQueue.EnqueueAsync(workItem, stoppingToken);

                return false;

            case MoveActions.ImmediateRetry:

                return true;

            default:
                throw new InvalidOperationException(nameof(_config.ActionInException));
        }
    }
}
