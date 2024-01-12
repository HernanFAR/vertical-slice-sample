// ReSharper disable CheckNamespace

using VSlices.Base;
using VSlices.CrossCutting;
using VSlices.CrossCutting.FluentValidation;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="FluentValidationBehavior{TRequest, TResult}"/>
/// </summary>
public static class FluentValidationBehaviorExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddOpenFluentValidationBehavior(this IServiceCollection services)
    {
        return services.AddOpenPipelineBehavior(typeof(FluentValidationBehavior<,>));
    }

    /// <summary>
    /// Adds a concrete pipeline behavior to the service collection
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="requestType">Request to intercept with a <see cref="FluentValidationBehavior{TRequest, TResult}" /></param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddFluentValidationBehaviorFor(this IServiceCollection services,
        Type requestType)
    {
        var requestDefinition = requestType.GetInterfaces()
            .Where(x => x.IsGenericType)
            .SingleOrDefault(x => x.GetGenericTypeDefinition() == typeof(IFeature<>));

        if (requestDefinition is null)
        {
            throw new InvalidOperationException(
                $"{requestType.FullName} does not implement {typeof(IFeature<>).FullName}");
        }

        var pipelineBehaviorType = typeof(IPipelineBehavior<,>)
            .MakeGenericType(requestType, requestDefinition.GetGenericArguments()[0]);

        var fluentValidationBehaviorType = typeof(FluentValidationBehavior<,>)
            .MakeGenericType(requestType, requestDefinition.GetGenericArguments()[0]);

        return services.AddTransient(pipelineBehaviorType, fluentValidationBehaviorType);
    }

    /// <summary>
    /// Adds a concrete pipeline behavior to the service collection
    /// </summary>
    /// <typeparam name="T">Pipeline behavior type</typeparam>
    /// <param name="services">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddFluentValidationBehaviorFor<T>(this IServiceCollection services)
        where T : class
    {
        return services.AddFluentValidationBehaviorFor(typeof(T));
    }
}
