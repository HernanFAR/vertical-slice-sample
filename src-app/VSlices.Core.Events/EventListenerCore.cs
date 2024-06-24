using LanguageExt;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    readonly ILogger<EventListenerCore> _logger;
    readonly IServiceProvider _serviceProvider;
    readonly EventListenerConfiguration _config;
    readonly IEventQueue _eventQueue;
    readonly Dictionary<Guid, int> _retries = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EventListenerCore"/> class.
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="configOptions">Configuration</param>
    public EventListenerCore(ILogger<EventListenerCore> logger,
        IServiceProvider serviceProvider,
        EventListenerConfiguration configOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = configOptions;
        _eventQueue = serviceProvider.GetRequiredService<IEventQueue>();
    }

    /// <inheritdoc />
    public async Task ProcessEvents(CancellationToken cancellationToken)
    {
        using var source = new CancellationTokenSource();
        var runtime = Runtime.New(ActivityEnv.Default, source);

        await using (cancellationToken.Register(source.Cancel))
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
                        Fin<Unit> result = await publisher.PublishAsync(workItem, runtime);

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
    }

    async Task<bool> CheckRetry(IEvent workItem, CancellationToken stoppingToken)
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
