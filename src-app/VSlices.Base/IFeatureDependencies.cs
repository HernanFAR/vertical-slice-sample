using VSlices.Base.Builder;

namespace VSlices.Base;

/// <summary>
/// Specifies dependencies in a given <see cref="IFeature{TResult}" />
/// </summary>
public interface IFeatureDependencies<TFeature, TResult>
    where TFeature : IFeature<TResult>
{
    /// <summary>
    /// Defines the dependencies for the feature
    /// </summary>
    static abstract void DefineDependencies(IFeatureStartBuilder<TFeature, TResult> define);
}

/// <summary>
/// Specifies dependencies in a given <see cref="IFeature{TResult}" />
/// </summary>
public interface IFeatureDependencies<TFeature> : IFeatureDependencies<TFeature, Unit>
    where TFeature : IFeature<Unit>;
