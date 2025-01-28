namespace VSlices.CrossCutting.BackgroundTaskListener;

/// <summary>
/// Defines a background task listener with an unspecified provider
/// </summary>
public interface IBackgroundTaskListener
{
    /// <summary>
    /// Starts the execution of the registered jobs
    /// </summary>
    ValueTask ExecuteRegisteredJobs(CancellationToken cancellationToken);

}

/// <summary>
/// Defines a task that executes in the background using <see cref="IBackgroundTaskListener"/>
/// </summary>
public interface IBackgroundTask
{
    /// <summary>
    /// An unique identifier for the task execution
    /// </summary>
    string Identifier { get; }

    /// <summary>
    /// Start the execution of the task
    /// </summary>
    ValueTask ExecuteAsync(CancellationToken cancellationToken);
}
