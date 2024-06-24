using VSlices.Core.Builder;
using VSlices.Core.Stream;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="FeatureBuilder" /> extensions for <see cref="IStreamHandler{TRequest,TResult}"/>
/// </summary>
public static class StreamHandlerExtensions
{
    /// <summary>
    /// Adds <typeparamref name="T"/> as <see cref="IStreamHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <typeparam name="T">The endpoint definition to be added</typeparam>
    /// <param name="featureBuilder">Service collection</param>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddStreamHandler<T>(this FeatureBuilder featureBuilder)
        => featureBuilder.AddStreamHandler(typeof(T));

    /// <summary>
    /// Adds an the specified <see cref="Type"/> as <see cref="IStreamHandler{TRequest,TResult}"/> to the service collection.
    /// </summary>
    /// <param name="featureBuilder">Service collection</param>
    /// <param name="handlerType">The endpoint definition to be added</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>Service collection</returns>
    public static FeatureBuilder AddStreamHandler(this FeatureBuilder featureBuilder,
        Type handlerType)
    {
        Type handlerInterface = handlerType
            .GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IStreamHandler<,>)) 
            ?? throw new InvalidOperationException(
                $"The type {handlerType.FullName} does not implement {typeof(IStreamHandler<,>).FullName}");

        featureBuilder.Services.AddTransient(handlerInterface, handlerType);

        return featureBuilder;

    }
}
