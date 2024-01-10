// ReSharper disable CheckNamespace

using VSlices.CrossCutting.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="LoggingBehavior{TRequest,TResult}"/>
/// </summary>
public static class LoggingBehaviorExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="exceptionHandlingBehavior">Exception handling behavior</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddOpenLoggingBehavior(this IServiceCollection services,
        Type exceptionHandlingBehavior)
    {
        if (!exceptionHandlingBehavior.IsAssignableTo(typeof(LoggingBehavior<,>)))
        {
            throw new InvalidOperationException(
                $"Type {exceptionHandlingBehavior.FullName} must inherit from {typeof(LoggingBehavior<,>).FullName}");
        }

        return services.AddOpenPipelineBehavior(exceptionHandlingBehavior);
    }
}
