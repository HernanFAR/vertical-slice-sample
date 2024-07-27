using VSlices.Core.Events;
using VSlices.Core.Events.Strategies;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="ReflectionEventRunner"/>.
/// </summary>
public static class ReflectionRunnerExtensions
{
    /// <summary>
    /// Add <see cref="ReflectionEventRunner"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>Default publishing strategy is <see cref="AwaitForEachStrategy"/></remarks>
    /// <param name="services">Service Collection</param>
    /// <param name="strategy">Publishing strategy</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddReflectionEventRunner(this IServiceCollection services,
        IPublishingStrategy? strategy = null)
    {
        strategy ??= new AwaitForEachStrategy();

        services.AddEventRunner<ReflectionEventRunner>();
        services.AddSingleton(strategy);

        return services;
    }
}
