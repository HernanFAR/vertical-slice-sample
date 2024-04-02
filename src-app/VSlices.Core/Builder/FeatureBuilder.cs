using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;

namespace VSlices.Core.Builder;

/// <summary>
/// Specifies a fluent api to define dependencies of a non specified <see cref="IFeature{TResult}" />
/// </summary>
public sealed class FeatureBuilder
{
    /// <summary>
    /// Initialize a <see cref="FeatureBuilder" /> using the specified <see cref="IServiceCollection" />
    /// </summary>
    /// <param name="serviceCollection"></param>
    public FeatureBuilder(IServiceCollection serviceCollection)
    {
        Services = serviceCollection;
    }

    /// <summary>
    /// The original <see cref="IServiceCollection" />
    /// </summary>
    public IServiceCollection Services { get; }

}
