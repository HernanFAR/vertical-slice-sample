using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.CrossCutting.Pipeline;
using VSlices.CrossCutting.Pipeline.FluentValidation;

namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureBuilder"/> extensions for <see cref="FluentValidationBehavior{TRequest, TResult}"/>
/// </summary>
public static class FluentValidationBehaviorExtensions
{
    /// <summary>
    /// Adds a concrete pipeline behavior to the service collection
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <param name="implValidatorType">Request to intercept with a <see cref="FluentValidationBehavior{TRequest, TResult}" /></param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static FeatureBuilder AddFluentValidationBehavior(this FeatureBuilder featureBuilder,
        Type implValidatorType)
    {
        var validatorType = implValidatorType.GetInterfaces()
            .Where(x => x.IsGenericType)
            .SingleOrDefault(x => x.GetGenericTypeDefinition() == typeof(IValidator<>))
            ?? throw new InvalidOperationException(
                $"{implValidatorType.FullName} does not implement {typeof(IValidator<>).FullName}");

        var requestType = validatorType.GetGenericArguments()[0];

        var implFeatureType = requestType.GetInterfaces()
            .Where(x => x.IsGenericType)
            .SingleOrDefault(x => x.GetGenericTypeDefinition() == typeof(IFeature<>))
            ?? throw new InvalidOperationException(
                $"{requestType} does not implement {typeof(IFeature<>).FullName}");

        featureBuilder.Services.AddTransient(validatorType, implValidatorType);

        var pipelineBehaviorType = typeof(IPipelineBehavior<,>)
            .MakeGenericType(requestType, implFeatureType.GetGenericArguments()[0]);

        var fluentValidationBehaviorType = typeof(FluentValidationBehavior<,>)
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
    public static FeatureBuilder AddFluentValidationBehavior<TValidator>(this FeatureBuilder services)
        where TValidator : class
    {
        return services.AddFluentValidationBehavior(typeof(TValidator));
    }
}
