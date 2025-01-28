using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Core;
using VSlices.Core.Presentation;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// <see cref="IEndpointRouteBuilder"/> extensions to expose <see cref="IEndpointIntegrator" /> and
/// <see cref="IEndpointIntegrator"/> in the <see cref="IServiceProvider"/>
/// </summary>
public static class AspNetCoreIntegrationExtensions
{
    /// <summary>
    /// Uses the endpoint definitions to Define the endpoints of the application.
    /// </summary>
    /// <param name="app">Endpoint route builder</param>
    public static void UseEndpointDefinitions(this IEndpointRouteBuilder app)
    {
        using IServiceScope services = app.ServiceProvider.CreateScope();

        var endpoints = services.ServiceProvider
                                .GetServices<IIntegrator>()
                                .OfType<IEndpointIntegrator>();

        foreach (IEndpointIntegrator endpoint in endpoints)
        {
            endpoint.Define(app);
        }
    }
}
