using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Presentation;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// <see cref="IEndpointRouteBuilder"/> extensions to expose <see cref="IEndpointDefinition" /> and
/// <see cref="IEndpointDefinition"/> in the <see cref="IServiceProvider"/>
/// </summary>
public static class AspNetCoreIntegrationExtensions
{
    /// <summary>
    /// Uses the endpoint definitions to define the endpoints of the application.
    /// </summary>
    /// <param name="app">Endpoint route builder</param>
    public static void UseEndpointDefinitions(this IEndpointRouteBuilder app)
    {
        using IServiceScope services = app.ServiceProvider.CreateScope();

        IEnumerable<IEndpointDefinition> endpoints = services.ServiceProvider.GetServices<IEndpointDefinition>();

        foreach (IEndpointDefinition endpoint in endpoints)
        {
            endpoint.Define(app);
        }
    }
}
