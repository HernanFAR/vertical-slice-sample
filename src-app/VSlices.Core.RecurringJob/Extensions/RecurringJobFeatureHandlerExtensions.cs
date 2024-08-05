using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Builder;
using VSlices.Core.RecurringJob;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// Recurring Job Feature RequestHandler Extensions
/// </summary>
public static class RecurringJobFeatureHandlerExtensions
{
    /// <summary>
    /// Adds a <see cref="IRecurringJobDefinition"/> to the service collection
    /// </summary>
    public static FeatureDefinition<,> AddRecurringJob<T>(this FeatureDefinition<,> definition)
        where T : class, IRecurringJobDefinition
    {
        definition.Services.AddSingleton<IRecurringJobDefinition, T>();

        return definition;
    }
}
