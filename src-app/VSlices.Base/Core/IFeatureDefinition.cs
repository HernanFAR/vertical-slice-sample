using VSlices.Base.CrossCutting;
using VSlices.Base.Definitions;

namespace VSlices.Base.Core;

/// <summary>
/// Assembles the elements of a feature
/// </summary>
public interface IFeatureDefinition
{
    /// <summary>
    /// Defines a feature with expected input and output, an associated <see cref="IBehavior{TInput,TOutput}"/>
    /// and an optional <see cref="IIntegrator" />, extensible via <see cref="IBehaviorInterceptor{TRequest,TResult}"/>
    /// </summary>
    static abstract Unit Define(FeatureComposer starting);
}
