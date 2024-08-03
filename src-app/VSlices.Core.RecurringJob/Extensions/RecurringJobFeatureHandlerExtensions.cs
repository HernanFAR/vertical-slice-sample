using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.RecurringJob;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// Recurring Job Feature Handler Extensions
/// </summary>
public static class RecurringJobFeatureHandlerExtensions
{
    /// <summary>
    /// Adds a <see cref="IRecurringJobDefinition"/> to the service collection
    /// </summary>
    public static FeatureBuilder AddRecurringJob<T>(this FeatureBuilder builder)
        where T : class, IRecurringJobDefinition
    {
        builder.Services.AddSingleton<IRecurringJobDefinition, T>();

        return builder;
    }
}
