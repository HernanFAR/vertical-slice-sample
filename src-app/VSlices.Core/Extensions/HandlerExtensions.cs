using VSlices.Core;
// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="IHandler{TRequest,TResult}"/>
/// </summary>
public static class HandlerExtensions
{
    /// <summary>
    /// Adds <typeparamref name="T"/> as <see cref="IHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <typeparam name="T">The endpoint definition to be added</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddHandler<T>(this IServiceCollection services)
    {
        return services.AddHandler(typeof(T));
    }

    /// <summary>
    /// Adds an the specified <see cref="Type"/> as <see cref="IHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="handlerType">The endpoint definition to be added</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddHandler(this IServiceCollection services,
        Type handlerType)
    {
        var handlerInterface = handlerType.GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IHandler<,>));

        if (handlerInterface is null)
        {
            throw new InvalidOperationException(
                $"The type {handlerType.FullName} does not implement {typeof(IHandler<,>).FullName}");
        }

        services.AddTransient(handlerInterface, handlerType);

        return services;
    }
}
