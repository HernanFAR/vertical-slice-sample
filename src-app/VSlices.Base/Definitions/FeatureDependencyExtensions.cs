using VSlices.Base.Core;
using VSlices.Base.Definitions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A delegate to compose the features to expose in the system.
/// </summary>
/// <param name="byExposing"></param>
public delegate void ComposerDelegate(OrchestratorComposer byExposing);

/// <summary>
/// <see cref="IServiceCollection"/> extensions for <see cref="IFeatureDefinition"/>
/// </summary>
public static class FeatureDependencyExtensions
{
    /// <summary>
    /// Allows an orchestration of features by exposing them in the system.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="composer"></param>
    /// <returns></returns>
    public static IServiceCollection Orchestrate(this IServiceCollection services, ComposerDelegate composer)
    {
        composer(new OrchestratorComposer(services));

        return services;
    }
}
