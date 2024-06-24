using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.CrossCutting.StreamPipeline;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureBuilder" /> extensions to simplify <see cref="IFeature{TResult}" />'s 
/// <see cref="IStreamPipelineBehavior{TRequest,TResult}" /> definitions
/// </summary>
public static class StreamPipelineBehaviorExtensions
{
    /// <summary>
    /// Adds <typeparamref name="T"/> as <see cref="IStreamPipelineBehavior{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <typeparam name="T">The endpoint definition to be added</typeparam>
    /// <param name="featureBuilder">Service collection</param>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddStreamPipeline<T>(this FeatureBuilder featureBuilder)
        => featureBuilder.AddStreamPipeline(typeof(T));

    /// <summary>
    /// Adds an the specified <see cref="Type"/> as <see cref="IHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <param name="featureBuilder">Service collection</param>
    /// <param name="handlerType">The endpoint definition to be added</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddStreamPipeline(this FeatureBuilder featureBuilder,
        Type handlerType)
    {
        var pipelineInterface = handlerType.GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IStreamPipelineBehavior<,>))
            ?? throw new InvalidOperationException(
                $"The type {handlerType.FullName} does not implement {typeof(IStreamPipelineBehavior<,>).FullName}");

        featureBuilder.Services.AddTransient(pipelineInterface, handlerType);

        return featureBuilder;

    }
}
