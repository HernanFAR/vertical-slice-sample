using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;

namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureBuilder" /> extensions to simplify <see cref="IFeature{TResult}" />'s 
/// <see cref="IHandler{TFeature, TResult}" /> definitions
/// </summary>
public static class HandlerExtensions
{
    /// <summary>
    /// Adds <typeparamref name="T"/> as <see cref="IHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <typeparam name="T">The endpoint definition to be added</typeparam>
    /// <param name="featureBuilder">Service collection</param>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddHandler<T>(this FeatureBuilder featureBuilder)
        => featureBuilder.AddHandler(typeof(T));

    /// <summary>
    /// Adds an the specified <see cref="Type"/> as <see cref="IHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <param name="featureBuilder">Service collection</param>
    /// <param name="handlerType">The endpoint definition to be added</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddHandler(this FeatureBuilder featureBuilder,
        Type handlerType)
    {
        var handlerInterface = handlerType.GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IHandler<,>))
            ?? throw new InvalidOperationException(
                $"The type {handlerType.FullName} does not implement {typeof(IHandler<,>).FullName}");

        featureBuilder.Services.AddTransient(handlerInterface, handlerType);

        return featureBuilder;

    }
}

