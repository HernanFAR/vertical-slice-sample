using Cronos;
using Microsoft.Extensions.Logging;
using VSlices.CrossCutting.BackgroundTaskListener;

namespace VSlices.Core.RecurringJob;

/// <summary>
/// Listeners and executes recurring jobs in the background
/// </summary>
public sealed class RecurringJobBackgroundTask(IEnumerable<IRecurringJobDefinition> recurringJobs, 
                                               ILogger<RecurringJobBackgroundTask> logger,
                                               TimeProvider timeProvider) : IBackgroundTask
{
    private readonly IEnumerable<IRecurringJobDefinition> _recurringJobs = recurringJobs;
    private readonly ILogger<RecurringJobBackgroundTask> _logger = logger;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <inheritdoc />
    public string Identifier => nameof(RecurringJobBackgroundTask);

    /// <inheritdoc />
    public async ValueTask ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting recurring job background tasks... ");
        IEnumerable<RecurringJobWrapper> jobs = _recurringJobs.Select(x => new RecurringJobWrapper(x, _timeProvider));

        await Task.WhenAll(jobs.Select(async x => await x.ExecuteAsync(cancellationToken)));
    }
}

internal sealed class RecurringJobWrapper(IRecurringJobDefinition recurringJob, TimeProvider timeProvider) : IRecurringJobDefinition
{
    private readonly IRecurringJobDefinition _recurringJob = recurringJob;
    private readonly TimeProvider _timeProvider = timeProvider;

    public string Identifier => _recurringJob.Identifier;

    public CronExpression Cron => _recurringJob.Cron;

    public async ValueTask ExecuteAsync(CancellationToken cancellationToken = default)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            DateTime current = _timeProvider.GetUtcNow()
                                            .UtcDateTime;

            DateTimeOffset? nextExecution = Cron.GetNextOccurrence(current);

            if (nextExecution.HasValue is false) return;

            TimeSpan delay = nextExecution.Value - current;
            PeriodicTimer timer = new(delay);

            if (await timer.WaitForNextTickAsync(cancellationToken) is false) return;

            timer.Dispose();

            await _recurringJob.ExecuteAsync(cancellationToken);
        }
    }
}
