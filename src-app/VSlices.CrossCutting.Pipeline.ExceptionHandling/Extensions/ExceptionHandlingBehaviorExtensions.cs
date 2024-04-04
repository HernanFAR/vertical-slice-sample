using VSlices.Core.Builder;
using VSlices.CrossCutting.Pipeline.ExceptionHandling;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="FeatureBuilder"/> extensions for <see cref="AbstractExceptionHandlingBehavior{TRequest,TResult}"/>
/// </summary>
public static class ExceptionHandlingBehaviorExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static FeatureBuilder AddExceptionHandlingPipeline<T>(this FeatureBuilder featureBuilder) 
        => featureBuilder.AddExceptionHandlingPipeline(typeof(T));

    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <param name="exceptionHandlingBehavior">Behavior</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static FeatureBuilder AddExceptionHandlingPipeline(this FeatureBuilder featureBuilder,
        Type exceptionHandlingBehavior)
    {
        if (!exceptionHandlingBehavior.IsAssignableTo(typeof(AbstractExceptionHandlingBehavior<,>)))
        {
            throw new InvalidOperationException(
                $"Type {exceptionHandlingBehavior.FullName} must inherit from {typeof(AbstractExceptionHandlingBehavior<,>).FullName}");
        }

        return featureBuilder.AddPipeline(exceptionHandlingBehavior);
    }
}
