using Microsoft.Extensions.DependencyInjection;
using VSlices.CrossCutting.StreamPipeline;
using VSlices.CrossCutting.StreamPipeline.ExceptionHandling;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureBuilder"/> extensions for <see cref="AbstractExceptionHandlingStreamBehavior{TRequest,TResult}"/>
/// </summary>
public static class ExceptionHandlingBehaviorStreamExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static FeatureBuilder AddExceptionHandlingStreamPipeline<T>(this FeatureBuilder featureBuilder)
        => featureBuilder.AddExceptionHandlingStreamPipeline(typeof(T));

    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <param name="exceptionHandlingBehavior">Behavior</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static FeatureBuilder AddExceptionHandlingStreamPipeline(this FeatureBuilder featureBuilder,
        Type exceptionHandlingBehavior)
    {
        var pipelineInterface = exceptionHandlingBehavior.GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IStreamPipelineBehavior<,>))
            ?? throw new InvalidOperationException(
                $"The type {exceptionHandlingBehavior.FullName} does not implement {typeof(IStreamPipelineBehavior<,>).FullName}");

        var exceptionHandlingBehaviorBase = typeof(AbstractExceptionHandlingStreamBehavior<,>)
            .MakeGenericType(pipelineInterface.GetGenericArguments()[0], pipelineInterface.GetGenericArguments()[1]);

        if (!exceptionHandlingBehavior.IsAssignableTo(exceptionHandlingBehaviorBase))
        {
            throw new InvalidOperationException(
                $"Type {exceptionHandlingBehavior.FullName} must inherit from {typeof(AbstractExceptionHandlingStreamBehavior<,>).FullName}");
        }

        featureBuilder.Services.AddTransient(pipelineInterface, exceptionHandlingBehavior);

        return featureBuilder;

    }
}
