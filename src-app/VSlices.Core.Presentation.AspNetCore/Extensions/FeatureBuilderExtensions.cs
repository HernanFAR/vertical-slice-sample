using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Core.Presentation;

namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureBuilder" /> extensions to simplify <see cref="IFeature{TResult}" />'s 
/// <see cref="IEndpointDefinition" /> definitions
/// </summary>
public static class FeatureBuilderExtensions
{
    /// <summary>
    /// Adds <typeparamref name="T"/> as <see cref="IEndpointDefinition"/> to the service collection.
    /// </summary>
    /// <remarks>This does not add dependencies if the class implements <see cref="IEndpointDefinition" /> or <see cref="IFeatureDependencies"/></remarks>
    /// <typeparam name="T">The endpoint definition to be added</typeparam>
    /// <param name="services">Feature builder</param>
    /// <returns>Feature builder</returns>
    public static FeatureBuilder AddEndpoint<T>(this FeatureBuilder services)
        where T : IEndpointDefinition
    {
        return services.AddEndpoint(typeof(T));
    }

    /// <summary>
    /// Adds the specified type as <see cref="IEndpointDefinition"/> to the service collection.
    /// </summary>
    /// <remarks>This does not add dependencies if the class implements <see cref="IEndpointDefinition" /> or <see cref="IFeatureDependencies"/></remarks>
    /// <param name="featureBuilder">Feature builder</param>
    /// <param name="type">The endpoint definition to be added</param>
    /// <returns>Feature builder</returns>
    public static FeatureBuilder AddEndpoint(this FeatureBuilder featureBuilder,
        Type type)
    {
        featureBuilder.Services.AddScoped(typeof(IEndpointDefinition), type);

        return featureBuilder;
    }
}
