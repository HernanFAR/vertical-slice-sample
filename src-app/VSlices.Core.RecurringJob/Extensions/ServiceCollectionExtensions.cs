using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSlices.Core.RecurringJob;
using VSlices.CrossCutting.BackgroundTaskListener;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for recurring jobs
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the recurring job listener to the service collection
    /// </summary>
    public static IServiceCollection AddRecurringJobListener(this IServiceCollection services) => 
        services.AddSingleton<IBackgroundTask, RecurringJobBackgroundTask>()
                .AddSingleton(TimeProvider.System);
}
