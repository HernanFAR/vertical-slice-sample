using VSlices.Core.Stream;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection" /> extensions for <see cref="IStreamRunner"/>
/// </summary>
public static class ReflectionStreamRunnerExtensions
{
    /// <summary>
    /// Add a reflection <see cref="IStreamRunner"/> implementation to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddReflectionStreamRunner(this IServiceCollection services)
    {
        return services.AddStreamRunner<ReflectionStreamRunner>();
    }
}
