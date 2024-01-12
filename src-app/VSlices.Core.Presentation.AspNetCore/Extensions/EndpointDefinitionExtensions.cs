﻿using VSlices.Core.Presentation.AspNetCore.WebApi;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="IEndpointDefinition"/>
/// </summary>
public static class EndpointDefinitionExtensions
{
    /// <summary>
    /// Adds <typeparamref name="T"/> as <see cref="ISimpleEndpointDefinition"/> to the service collection.
    /// </summary>
    /// <typeparam name="T">The endpoint definition to be added</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddEndpointDefinition<T>(this IServiceCollection services)
        where T : ISimpleEndpointDefinition
    {
        return services.AddEndpointDefinition(typeof(T));
    }

    /// <summary>
    /// Adds the specified type as <see cref="ISimpleEndpointDefinition"/> to the service collection.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="type">The endpoint definition to be added</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddEndpointDefinition(this IServiceCollection services,
        Type type)
    {
        return services.AddScoped(typeof(ISimpleEndpointDefinition), type);
    }
}
