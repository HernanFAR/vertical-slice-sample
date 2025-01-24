using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Core;

namespace VSlices.Base.Definitions;

/// <summary>
/// Provides methods to expose features in a system
/// </summary>
/// <param name="services">The service collection to add the features</param>
public sealed class OrchestratorComposer(IServiceCollection services)
{
    /// <summary>
    /// Exposes <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The feature to expose</typeparam>
    /// <returns>The orchestrator to expose more features</returns>
    public OrchestratorComposer Feature<T>()
        where T : IFeatureDefinition =>
        Feature(typeof(T));

    /// <summary>
    /// Exposes the specified type
    /// </summary>
    /// <param name="type">The feature type to expose</param>
    /// <returns>The orchestrator to expose more features</returns>
    /// <exception cref="InvalidOperationException">If the specified type does not implement <see cref="IFeatureDefinition"/></exception>
    public OrchestratorComposer Feature(Type type)
    {
        if (type.IsAssignableTo(typeof(IFeatureDefinition)) is false)
        {
            throw new InvalidOperationException($"{type.FullName} does not implement {nameof(IFeatureDefinition)}");
        }

        MethodInfo defineDependenciesMethod =
            type.GetMethod(nameof(IFeatureDefinition.Define)) ??
            throw new InvalidOperationException($"{type.FullName} does not implement " +
                                                $"{nameof(IFeatureDefinition)}.{nameof(IFeatureDefinition.Define)}");

        defineDependenciesMethod.Invoke(null, [new FeatureComposer(services)]);

        return this;
    }

    /// <summary>
    /// Exposes classes that implements <see cref="IFeatureDefinition"/>, from the assembly of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to scan for <see cref="IFeatureDefinition"/> implementations</typeparam>
    /// <returns>The orchestrator to expose more features</returns>
    public OrchestratorComposer AllFeaturesFromAssemblyContaining<T>() => AllFeaturesFromAssemblyContaining(typeof(T));

    /// <summary>
    /// Exposes classes that implements <see cref="IFeatureDefinition"/>, from the assembly of the specified type.
    /// </summary>
    /// <param name="from">The type to scan for <see cref="IFeatureDefinition"/> implementations</param>
    /// <returns>The orchestrator to expose more features</returns>
    public OrchestratorComposer AllFeaturesFromAssemblyContaining(Type from)
    {
        from.Assembly.ExportedTypes
            .Where(e => e.IsAssignableTo(typeof(IFeatureDefinition)))
            .Where(e => e is { IsAbstract: false, IsInterface: false })
            .Select(Feature)
            .Consume();

        return this;
    }
}
