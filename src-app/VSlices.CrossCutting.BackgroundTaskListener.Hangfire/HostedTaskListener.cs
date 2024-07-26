using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace VSlices.CrossCutting.BackgroundTaskListener.Hangfire;

/// <summary>
/// A background task listener using Hangfire
/// </summary>
public sealed class HangfireTaskListener(
    IBackgroundJobClient hangfireJobClient,
    IEnumerable<IBackgroundTask> backgroundTasks)
    : BackgroundService, IBackgroundTaskListener
{
    private readonly IBackgroundJobClient _hangfireJobClient = hangfireJobClient;
    private readonly IEnumerable<IBackgroundTask> _backgroundTasks = backgroundTasks;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExecuteRegisteredJobs(stoppingToken);
    }

    /// <inheritdoc />
    public async ValueTask ExecuteRegisteredJobs(CancellationToken cancellationToken)
    {
        await Task.WhenAll(_backgroundTasks.Select(task =>
        {
            Type jobType = task.GetType();
            MethodInfo? jobMethod = jobType.GetMethod(nameof(IBackgroundTask.ExecuteAsync));
            Job hangFireJob = new(jobType, jobMethod, cancellationToken);

            _hangfireJobClient.Create(hangFireJob, new EnqueuedState(EnqueuedState.DefaultQueue));

            return Task.CompletedTask;
        }));
    }
}
