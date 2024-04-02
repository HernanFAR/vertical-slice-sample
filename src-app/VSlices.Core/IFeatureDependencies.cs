using VSlices.Base;
using VSlices.Core.Builder;

namespace VSlices.Core;

/// <summary>
/// Specifies dependencies in a given <see cref="IFeature{TResult}" />
/// </summary>
public interface IFeatureDependencies
{
    /// <summary>
    /// Defines the dependencies for the use case
    /// </summary>
    /// <param name="featureBuilder">Feature builder</param>
    static abstract void DefineDependencies(FeatureBuilder featureBuilder);
}
