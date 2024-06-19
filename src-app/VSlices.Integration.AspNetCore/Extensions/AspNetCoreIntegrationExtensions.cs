using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Presentation;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// <see cref="IEndpointRouteBuilder"/> extensions to expose <see cref="IEndpoint" /> and
/// <see cref="IEndpoint"/> in the <see cref="IServiceProvider"/>
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

        IEnumerable<IEndpoint> endpoints = services.ServiceProvider.GetServices<IEndpoint>();

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.DefineEndpoint(app);
        }
    }
}
