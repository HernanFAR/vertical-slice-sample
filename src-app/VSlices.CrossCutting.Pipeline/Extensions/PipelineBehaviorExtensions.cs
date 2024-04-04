using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.CrossCutting.Pipeline;

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
    /// <typeparam name="T">The endpoint definition to be added</typeparam>
    /// <param name="featureBuilder">Service collection</param>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddPipeline<T>(this FeatureBuilder featureBuilder)
        => featureBuilder.AddPipeline(typeof(T));

    /// <summary>
    /// Adds an the specified <see cref="Type"/> as <see cref="IHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <param name="featureBuilder">Service collection</param>
    /// <param name="handlerType">The endpoint definition to be added</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddPipeline(this FeatureBuilder featureBuilder,
        Type handlerType)
    {
        var handlerInterface = handlerType.GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

        if (handlerInterface is null)
        {
            throw new InvalidOperationException(
                $"The type {handlerType.FullName} does not implement {typeof(IPipelineBehavior<,>).FullName}");
        }

        featureBuilder.Services.AddTransient(handlerInterface, handlerType);

        return featureBuilder;

    }
}
