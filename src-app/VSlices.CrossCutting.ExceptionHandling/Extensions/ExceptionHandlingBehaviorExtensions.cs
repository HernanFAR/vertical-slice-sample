// ReSharper disable CheckNamespace

using VSlices.CrossCutting.ExceptionHandling;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="AbstractExceptionHandlingBehavior{TRequest,TResult}"/>
/// </summary>
public static class ExceptionHandlingBehaviorExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="exceptionHandlingBehavior">Exception handling behavior</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddOpenExceptionHandlingBehavior(this IServiceCollection services,
        Type exceptionHandlingBehavior)
    {
        if (!exceptionHandlingBehavior.IsAssignableTo(typeof(AbstractExceptionHandlingBehavior<,>)))
        {
            throw new InvalidOperationException(
                $"Type {exceptionHandlingBehavior.FullName} must inherit from {typeof(AbstractExceptionHandlingBehavior<,>).FullName}");
        }

        return services.AddOpenPipelineBehavior(exceptionHandlingBehavior);
    }
}
