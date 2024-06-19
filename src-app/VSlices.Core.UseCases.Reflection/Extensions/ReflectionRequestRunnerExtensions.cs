using VSlices.Core.UseCases;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection" /> extensions for <see cref="IRequestRunner"/>
/// </summary>
public static class ReflectionRequestRunnerExtensions
{
    /// <summary>
    /// Add a reflection <see cref="IRequestRunner"/> implementation to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddReflectionRequestRunner(this IServiceCollection services)
    {
        return services.AddRequestRunner<ReflectionRequestRunner>();
    }
}
