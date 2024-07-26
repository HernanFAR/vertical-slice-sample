using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.CrossCutting.Pipeline;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureBuilder" /> extensions to simplify <see cref="IFeature{TResult}" />'s 
/// <see cref="IPipelineBehavior{TFeature, TResult}" /> definitions
/// </summary>
public static class PipelineBehaviorExtensions
{
    /// <summary>
    /// Adds <typeparamref name="T"/> as <see cref="IPipelineBehavior{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <typeparam name="T">The endpoint definition to add</typeparam>
    /// <param name="featureBuilder">Service collection</param>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddPipeline<T>(this FeatureBuilder featureBuilder)
        => featureBuilder.AddPipeline(typeof(T));

    /// <summary>
    /// Adds the specified <see cref="Type"/> as <see cref="IHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <param name="featureBuilder">Service collection</param>
    /// <param name="handlerType">The endpoint definition to add</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddPipeline(this FeatureBuilder featureBuilder,
        Type handlerType)
    {
        var pipelineInterface = handlerType.GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
            ?? throw new InvalidOperationException(
                $"The type {handlerType.FullName} does not implement {typeof(IPipelineBehavior<,>).FullName}");

        featureBuilder.Services.AddTransient(pipelineInterface, handlerType);

        return featureBuilder;

    }
}
