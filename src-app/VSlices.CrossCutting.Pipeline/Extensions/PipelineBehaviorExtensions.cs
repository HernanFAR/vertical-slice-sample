// ReSharper disable CheckNamespace
using VSlices.CrossCutting;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection" /> extensions for <see cref="IPipelineBehavior{TRequest,TResponse}"/>
/// </summary>
public static class PipelineBehaviorExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="type"></param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddOpenPipelineBehavior(this IServiceCollection services,
        Type type)
    {
        var implementsPipelineBehavior = type.GetInterfaces()
            .Where(x => x.IsGenericType)
            .Any(x => x.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

        if (!implementsPipelineBehavior)
        {
            throw new InvalidOperationException(
                $"The type {type.FullName} does not implement {typeof(IPipelineBehavior<,>).FullName}");
        }

        services.AddTransient(typeof(IPipelineBehavior<,>), type);

        return services;
    }
}
