using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Definitions;
using VSlices.CrossCutting.Interceptor.FluentValidation;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="InterceptorChain"/> extensions for <see cref="FluentValidationInterceptor{TIn,TOut}"/>
/// </summary>
public static class FluentValidationBehaviorExtensions
{
    /// <summary>
    /// Adds a fluent validation behavior in the pipeline execution related to this specific handler.
    /// </summary>
    public static InterceptorChain AddFluentValidationUsing<T>(this InterceptorChain handlerEffects)
        where T : IValidator
    {
        Type implType = typeof(T); 
        Type interfaceType = typeof(IValidator<>).MakeGenericType(handlerEffects.InType);

        bool isFeatureValidator = implType.GetInterfaces()
                                          .Any(x => x == interfaceType);

        if (isFeatureValidator is false)
        {
            throw new InvalidOperationException($"{implType.FullName} does not implement {interfaceType.FullName}");
        }

        handlerEffects.Add(typeof(FluentValidationInterceptor<,>))
                 .Services.AddTransient(interfaceType, implType);

        return handlerEffects;
    }
}
