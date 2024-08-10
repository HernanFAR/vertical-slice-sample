using VSlices.Base.Builder;

namespace VSlices.Base.Core;

/// <summary>
/// Specifies dependencies in a given <see cref="IFeature{TResult}" />
/// </summary>
public interface IFeatureDependencies<TFeature, TResult>
    where TFeature : IFeature<TResult>
{
    /// <summary>
    /// Defines the dependencies for the feature
    /// </summary>
    static abstract void DefineDependencies(IFeatureStartBuilder<TFeature, TResult> feature);
}

/// <summary>
/// Specifies dependencies in a given <see cref="IFeature{TResult}" />
/// </summary>
public interface IFeatureDependencies<TFeature> : IFeatureDependencies<TFeature, Unit>
    where TFeature : IFeature<Unit>;

/// <summary>
/// Not indented to be used in development
/// </summary>
public interface IFeatureDependencies;