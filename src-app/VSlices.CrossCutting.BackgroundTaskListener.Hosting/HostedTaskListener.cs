using Microsoft.Extensions.Hosting;

namespace VSlices.CrossCutting.BackgroundTaskListener.Hosting;

/// <summary>
/// A background task listener using MS.Ext.Hosting
/// </summary>
public sealed class HostedTaskListener(IEnumerable<IBackgroundTask> backgroundTasks)
    : BackgroundService, IBackgroundTaskListener
{
    private readonly IEnumerable<IBackgroundTask> _backgroundTasks = backgroundTasks;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExecuteRegisteredJobs(stoppingToken);
    }

    /// <inheritdoc />
    public async ValueTask ExecuteRegisteredJobs(CancellationToken cancellationToken)
    {
        await Task.WhenAll(_backgroundTasks.Select(task => task.ExecuteAsync(cancellationToken).AsTask()));
    }
}
