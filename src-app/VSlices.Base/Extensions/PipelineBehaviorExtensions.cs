using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Builder;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureDefinition{TFeature,TResult}" /> extensions to simplify <see cref="IFeature{TResult}" />'s 
/// <see cref="IPipelineBehavior{TFeature, TResult}" /> definitions
/// </summary>
public static class PipelineBehaviorExtensions
{
    /// <summary>
    /// Adds <typeparamref name="T"/> as <see cref="IPipelineBehavior{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <typeparam name="T">The endpoint definition to add</typeparam>
    /// <param name="featureDefinition">Service collection</param>
    /// <returns>Service collection</returns>
    public static FeatureDefinition<,> AddPipeline<T>(this FeatureDefinition<,> featureDefinition)
        => featureDefinition.AddPipeline(typeof(T));

    /// <summary>
    /// Adds the specified <see cref="Type"/> as <see cref="IHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <param name="featureDefinition">Service collection</param>
    /// <param name="handlerType">The endpoint definition to add</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>Service collection</returns>
    public static FeatureDefinition<,> AddPipeline(this FeatureDefinition<,> featureDefinition,
                                                    Type handlerType)
    {
        var pipelineInterface = handlerType.GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
            ?? throw new InvalidOperationException(
                $"The type {handlerType.FullName} does not implement {typeof(IPipelineBehavior<,>).FullName}");

        featureDefinition.Services.AddTransient(pipelineInterface, handlerType);

        return featureDefinition;

    }
}
