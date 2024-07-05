using Microsoft.Extensions.DependencyInjection;
using VSlices.CrossCutting.Pipeline;
using VSlices.CrossCutting.Pipeline.ExceptionHandling;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

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
    public static FeatureBuilder AddExceptionHandling<T>(this FeatureBuilder featureBuilder)
        where T : IPipelineBehavior
        => featureBuilder.AddExceptionHandling(typeof(T));

    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <param name="exceptionHandlingBehavior">Behavior</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static FeatureBuilder AddExceptionHandling(this FeatureBuilder featureBuilder,
        Type exceptionHandlingBehavior)
    {
        var pipelineInterface = exceptionHandlingBehavior.GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
            ?? throw new InvalidOperationException(
                $"The type {exceptionHandlingBehavior.FullName} does not implement {typeof(IPipelineBehavior<,>).FullName}");

        var exceptionHandlingBehaviorBase = typeof(AbstractExceptionHandlingBehavior<,>)
            .MakeGenericType(pipelineInterface.GetGenericArguments()[0], pipelineInterface.GetGenericArguments()[1]);

        if (!exceptionHandlingBehavior.IsAssignableTo(exceptionHandlingBehaviorBase))
        {
            throw new InvalidOperationException(
                $"Type {exceptionHandlingBehavior.FullName} must inherit from {typeof(AbstractExceptionHandlingBehavior<,>).FullName}");
        }

        featureBuilder.Services.AddTransient(pipelineInterface, exceptionHandlingBehavior);

        return featureBuilder;

    }
}
