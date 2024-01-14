using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Presentation;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// <see cref="IEndpointRouteBuilder"/> extensions to expose <see cref="IEndpointDefinition" /> and
/// <see cref="ISimpleEndpointDefinition"/> in the <see cref="IServiceProvider"/>
/// </summary>
public static class AspNetCoreIntegrationExtensions
{
    /// <summary>
    /// Uses the endpoint definitions to define the endpoints of the application.
    /// </summary>
    /// <param name="app">Endpoint route builder</param>
    public static void UseEndpointDefinitions(this IEndpointRouteBuilder app)
    {
        using var services = app.ServiceProvider.CreateScope();

        var endpoints = services.ServiceProvider.GetServices<ISimpleEndpointDefinition>();

        foreach (var endpoint in endpoints)
        {
            endpoint.DefineEndpoint(app);
        }
    }
}
