using Microsoft.AspNetCore.Routing;

namespace VSlices.Core.Presentation.AspNetCore.WebApi;

/// <summary>
/// Defines an endpoint of a use case without dependencies
/// </summary>
/// <remarks>If you need to specify dependencies, use <see cref="IEndpointDefinition"/></remarks>
public interface ISimpleEndpointDefinition
{
    /// <summary>
    /// Defines the endpoint of the use case.
    /// </summary>
    /// <param name="builder">Endpoint route builder</param>
    void DefineEndpoint(IEndpointRouteBuilder builder);
}

/// <summary>
/// Defines an endpoint of a use case with dependencies
/// </summary>
public interface IEndpointDefinition : ISimpleEndpointDefinition, IFeatureDependencyDefinition
{
}
