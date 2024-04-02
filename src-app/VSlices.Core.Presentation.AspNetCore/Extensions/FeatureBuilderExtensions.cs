using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Presentation;

namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureBuilder"/> extensions for <see cref="IEndpoint"/>
/// </summary>
public static class FeatureBuilderExtensions
{
    /// <summary>
    /// Adds <typeparamref name="T"/> as <see cref="ISimpleEndpoint"/> to the service collection.
    /// </summary>
    /// <remarks>This does not add dependencies if the class implements <see cref="IEndpoint" /> or <see cref="IFeatureDependencies"/></remarks>
    /// <typeparam name="T">The endpoint definition to be added</typeparam>
    /// <param name="services">Feature builder</param>
    /// <returns>Feature builder</returns>
    public static FeatureBuilder AddEndpoint<T>(this FeatureBuilder services)
        where T : ISimpleEndpoint
    {
        return services.AddEndpoint(typeof(T));
    }

    /// <summary>
    /// Adds the specified type as <see cref="ISimpleEndpoint"/> to the service collection.
    /// </summary>
    /// <remarks>This does not add dependencies if the class implements <see cref="IEndpoint" /> or <see cref="IFeatureDependencies"/></remarks>
    /// <param name="featureBuilder">Feature builder</param>
    /// <param name="type">The endpoint definition to be added</param>
    /// <returns>Feature builder</returns>
    public static FeatureBuilder AddEndpoint(this FeatureBuilder featureBuilder,
        Type type)
    {
        featureBuilder.Services.AddScoped(typeof(ISimpleEndpoint), type);

        return featureBuilder;
    }
}
