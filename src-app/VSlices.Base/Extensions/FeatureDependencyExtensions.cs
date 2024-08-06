using System.Reflection;
using VSlices.Base.Builder;
using VSlices.Base.Core;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="IFeatureDependencies{TFeature,TResult}"/>
/// </summary>
public static class FeatureDependencyExtensions
{
    /// <summary>
    /// Adds the dependencies defined in the <see cref="IFeatureDependencies{TFeature,TResult}"/> implementations from the
    /// <see cref="Assembly"/>'s <typeparamref name="TAnchor"></typeparamref>
    /// </summary>
    /// <typeparam name="TAnchor"></typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddFeatureDependenciesFromAssemblyContaining<TAnchor>(this IServiceCollection services)
    {
        return services.AddFeatureDependenciesFromAssemblyContaining(typeof(TAnchor));
    }

    /// <summary>
    /// Adds the dependencies defined in the <see cref="IFeatureDependencies{TFeature,TResult}"/> implementations from the
    /// <see cref="Assembly"/>'s specified <see cref="Type"/>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddFeatureDependenciesFromAssemblyContaining(this IServiceCollection services,
        Type type)
    {
        return services.AddFeatureDependenciesFromAssembly(type.Assembly);
    }

    /// <summary>
    /// Adds the dependencies defined in the <see cref="IFeatureDependencies{TFeature,TResult}"/> implementations from the specified
    /// <see cref="Assembly"/>
    /// </summary>
    /// <param name="assembly">Assembly to scan</param>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddFeatureDependenciesFromAssembly(this IServiceCollection services,
        Assembly assembly)
    {
        var types = assembly.ExportedTypes
            .Where(e => e.GetInterfaces()
                         .Where(e => e.IsGenericType)
                         .Any(e => e.GetGenericTypeDefinition() == typeof(IFeatureDependencies<,>)))
            .Where(e => e is { IsAbstract: false, IsInterface: false });

        foreach (var type in types)
        {
            services.AddFeatureDependency(type);
        }

        return services;
    }

    /// <summary>
    /// Adds the dependencies defined in the <typeparamref name="T"></typeparamref> implementation
    /// </summary>
    /// <typeparam name="T"><see cref="IFeatureDependencies{TFeature,TResult}"/> implementation</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddFeatureDependency<T>(this IServiceCollection services)
        where T : IFeatureDependencies
    {
        services.AddFeatureDependency(typeof(T));   
        
        return services;
    }

    /// <summary>
    /// Adds the dependencies defined in the <see cref="IFeatureDependencies{TFeature,TResult}"/> implementations from the
    /// specified <see cref="Type"/>
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="type"><see cref="IFeatureDependencies{TFeature,TResult}"/> implementation</param>
    /// <returns>Service collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddFeatureDependency(this IServiceCollection services,
        Type type)
    {
        var dependenciesInferface = type.GetInterfaces()
                                        .Where(e => e.IsGenericType)
                                        .SingleOrDefault(e => e.GetGenericTypeDefinition() ==
                                                              typeof(IFeatureDependencies<,>))
                                        ?? throw new InvalidOperationException(
                                            $"{type.FullName} does not implement IFeatureDependencies");

        var featureType = dependenciesInferface.GetGenericArguments()[0];
        var resultType = dependenciesInferface.GetGenericArguments()[1];

        var defineDependenciesMethod = type.GetMethod("DefineDependencies")
            ?? throw new InvalidOperationException(
                $"{type.FullName} does not implement IFeatureDependencies<,>.DefineDependencies");

        Type    featureDefinitionType = typeof(FeatureDefinition<,>).MakeGenericType(featureType, resultType);
        object? featureDefinition     = Activator.CreateInstance(featureDefinitionType, [services])
                                        ?? throw new InvalidOperationException(
                                            $"Cannot instantiate {featureDefinitionType.FullName}");

        defineDependenciesMethod!.Invoke(null, [featureDefinition]);
        
        return services;
    }
}
