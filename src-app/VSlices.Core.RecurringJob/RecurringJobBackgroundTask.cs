using Cronos;
using Microsoft.Extensions.Logging;
using VSlices.Base.Core;
using VSlices.CrossCutting.BackgroundTaskListener;

namespace VSlices.Core.RecurringJob;

/// <summary>
/// Listeners and executes recurring jobs in the background
/// </summary>
public sealed class RecurringJobBackgroundTask(IEnumerable<IIntegrator> recurringJobs, 
                                               ILogger<RecurringJobBackgroundTask> logger,
                                               TimeProvider timeProvider) : IBackgroundTask
{
    private readonly IEnumerable<IRecurringJobIntegrator> _recurringJobs = recurringJobs.OfType<IRecurringJobIntegrator>();
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

internal sealed class RecurringJobWrapper(IRecurringJobIntegrator recurringJob, TimeProvider timeProvider) : IRecurringJobIntegrator
{
    private readonly IRecurringJobIntegrator _recurringJob = recurringJob;
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
