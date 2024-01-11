using Microsoft.Extensions.DependencyInjection;

namespace VSlices.Core;

/// <summary>
/// Specifies dependencies in a given use case
/// </summary>
public interface IFeatureDependencyDefinition
{
    /// <summary>
    /// Defines the dependencies for the use case
    /// </summary>
    /// <param name="services">Service collection</param>
    static abstract void DefineDependencies(IServiceCollection services);
}
