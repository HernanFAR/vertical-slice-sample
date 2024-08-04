using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Core.UseCases;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureBuilder" /> extensions to simplify <see cref="IFeature{TResult}" />'s 
/// <see cref="IRequestHandler{TRequest}" /> definitions
/// </summary>
public static class HandlerExtensions
{
    /// <summary>
    /// Adds <typeparamref name="T"/> as <see cref="IRequestHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <typeparam name="T">The endpoint definition to add</typeparam>
    /// <param name="featureBuilder">Service collection</param>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddRequestHandler<T>(this FeatureBuilder featureBuilder)
        => featureBuilder.AddRequestHandler(typeof(T));

    /// <summary>
    /// Adds an the specified <see cref="Type"/> as <see cref="IRequestHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <param name="featureBuilder">Service collection</param>
    /// <param name="handlerType">The endpoint definition to add</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddRequestHandler(this FeatureBuilder featureBuilder,
        Type handlerType)
    {
        var handlerInterface = handlerType.GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            ?? throw new InvalidOperationException(
                $"The type {handlerType.FullName} does not implement {typeof(IRequestHandler<,>).FullName}");

        featureBuilder.Services.AddTransient(handlerInterface, handlerType);

        return featureBuilder;

    }
}

