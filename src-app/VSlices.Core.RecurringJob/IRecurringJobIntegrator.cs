using Cronos;
using VSlices.Base.Core;

namespace VSlices.Core.RecurringJob;

/// <summary>
/// Defines the class as a presentation for a recurring job
/// </summary>
public interface IRecurringJobIntegrator : IIntegrator
{
    /// <summary>
    /// Gets the identifier of the recurring job
    /// </summary>
    string Identifier { get; }

    /// <summary>
    /// Gets the cron expression of the recurring job
    /// </summary>
    CronExpression Cron { get; }

    /// <summary>
    /// Executes the recurring job
    /// </summary>
    ValueTask ExecuteAsync(CancellationToken cancellationToken = default);
}
