﻿using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Core;
using VSlices.Core.Builder;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="IFeatureDependencies"/>
/// </summary>
public static class FeatureDependencyExtensions
{
    /// <summary>
    /// Adds the dependencies defined in the <see cref="IFeatureDependencies"/> implementations from the
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
    /// Adds the dependencies defined in the <see cref="IFeatureDependencies"/> implementations from the
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
    /// Adds the dependencies defined in the <see cref="IFeatureDependencies"/> implementations from the specified
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
            .Where(e => typeof(IFeatureDependencies).IsAssignableFrom(e))
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
    /// <typeparam name="T"><see cref="IFeatureDependencies"/> implementation</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddFeatureDependency<T>(this IServiceCollection services)
        where T : IFeatureDependencies
    {
        T.DefineDependencies(new FeatureBuilder(services));

        return services;
    }

    /// <summary>
    /// Adds the dependencies defined in the <see cref="IFeatureDependencies"/> implementations from the
    /// specified <see cref="Type"/>
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="type"><see cref="IFeatureDependencies"/> implementation</param>
    /// <returns>Service collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddFeatureDependency(this IServiceCollection services,
        Type type)
    {
        if (!typeof(IFeatureDependencies).IsAssignableFrom(type))
        {
            throw new InvalidOperationException(
                $"{type.FullName} does not implement {nameof(IFeatureDependencies)}");
        }

        var defineDependenciesMethod = type.GetMethod(nameof(IFeatureDependencies.DefineDependencies));

        defineDependenciesMethod!.Invoke(null, new object?[] { new FeatureBuilder(services) });
        
        return services;
    }
}
