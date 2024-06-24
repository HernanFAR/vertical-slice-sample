using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Core.Stream;
using VSlices.CrossCutting.StreamPipeline;
using VSlices.CrossCutting.StreamPipeline.FluentValidation;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureBuilder"/> extensions for <see cref="FluentValidationStreamBehavior{TRequest, TResult}"/>
/// </summary>
public static class FluentValidationStreamBehaviorExtensions
{
    /// <summary>
    /// Adds a concrete pipeline behavior to the service collection
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <param name="implValidatorType">Request to intercept with a <see cref="FluentValidationStreamBehavior{TRequest, TResult}" /></param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static FeatureBuilder AddFluentValidationStreamBehavior(this FeatureBuilder featureBuilder,
        Type implValidatorType)
    {
        Type validatorType = implValidatorType.GetInterfaces()
                                 .Where(x => x.IsGenericType)
                                 .SingleOrDefault(x => x.GetGenericTypeDefinition() == typeof(IValidator<>))
                             ?? throw new InvalidOperationException(
                                 $"{implValidatorType.FullName} does not implement {typeof(IValidator<>).FullName}");

        Type requestType = validatorType.GetGenericArguments()[0];

        Type implFeatureType = requestType.GetInterfaces()
                                   .Where(x => x.IsGenericType)
                                   .SingleOrDefault(x => x.GetGenericTypeDefinition() == typeof(IStream<>))
                               ?? throw new InvalidOperationException(
                                   $"{requestType} does not implement {typeof(IStream<>).FullName}");

        featureBuilder.Services.AddTransient(validatorType, implValidatorType);

        Type pipelineBehaviorType = typeof(IStreamPipelineBehavior<,>)
            .MakeGenericType(requestType, implFeatureType.GetGenericArguments()[0]);

        Type fluentValidationBehaviorType = typeof(FluentValidationStreamBehavior<,>)
            .MakeGenericType(requestType, implFeatureType.GetGenericArguments()[0]);

        featureBuilder.Services.AddTransient(pipelineBehaviorType, fluentValidationBehaviorType);

        return featureBuilder;
    }

    /// <summary>
    /// Adds a concrete pipeline behavior to the service collection
    /// </summary>
    /// <typeparam name="TValidator">Pipeline behavior type</typeparam>
    /// <param name="services">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static FeatureBuilder AddFluentValidationStreamBehavior<TValidator>(this FeatureBuilder services)
        where TValidator : class
    {
        return services.AddFluentValidationStreamBehavior(typeof(TValidator));
    }
}
