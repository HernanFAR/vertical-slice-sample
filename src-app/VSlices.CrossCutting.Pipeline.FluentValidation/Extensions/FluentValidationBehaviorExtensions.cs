using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Builder;
using VSlices.CrossCutting.Pipeline.FluentValidation;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureDefinition{TFeature,TResult}"/> extensions for <see cref="FluentValidationBehavior{TRequest, TResult}"/>
/// </summary>
public static class FluentValidationBehaviorExtensions
{
    /// <summary>
    /// Adds a fluent validation behavior in the pipeline execution related to this specific handler.
    /// </summary>
    public static BehaviorChain AddFluentValidationUsing<T>(this BehaviorChain handlerEffects)
        where T : IValidator
    {
        Type implType = typeof(T); 
        Type interfaceType = typeof(IValidator<>).MakeGenericType(handlerEffects.FeatureType);

        bool isFeatureValidator = implType.GetInterfaces()
                                          .Any(x => x == interfaceType);

        if (isFeatureValidator is false)
        {
            throw new InvalidOperationException($"{implType.FullName} does not implement {interfaceType.FullName}");
        }

        handlerEffects.Add(typeof(FluentValidationBehavior<,>))
                 .Services.AddTransient(interfaceType, implType);

        return handlerEffects;
    }
}
