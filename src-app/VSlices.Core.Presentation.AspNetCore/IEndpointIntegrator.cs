using Microsoft.AspNetCore.Routing;
using VSlices.Base.Core;

namespace VSlices.Core.Presentation;

/// <summary>
/// Defines an endpoint of a use case without dependencies
/// </summary>
public interface IEndpointIntegrator : IIntegrator
{
    /// <summary>
    /// Defines the endpoint of the use case.
    /// </summary>
    /// <param name="builder">Endpoint route builder</param>
    void Define(IEndpointRouteBuilder builder);
}
