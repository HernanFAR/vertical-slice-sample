﻿using Sample.Core.Interfaces;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEndpointDefinition<TEndpointDefinition>(this IServiceCollection services)
        where TEndpointDefinition : class, IEndpointDefinition
    {
        services.AddSingleton(typeof(ISimpleEndpointDefinition), typeof(TEndpointDefinition));
        services.AddSingleton(typeof(IEndpointDefinition), typeof(TEndpointDefinition));

        TEndpointDefinition.DefineDependencies(services);

        return services;
    }

    public static IServiceCollection AddSimpleEndpointDefinition<TEndpointDefinition>(this IServiceCollection services)
        where TEndpointDefinition : class, ISimpleEndpointDefinition
    {
        services.AddSingleton(typeof(ISimpleEndpointDefinition), typeof(TEndpointDefinition));

        return services;
    }
}