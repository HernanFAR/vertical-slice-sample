using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base.CrossCutting;

namespace VSlices.Base.Builder;

/// <summary>
/// Define the behavior chain related to a concrete handler type
/// </summary>
public sealed class BehaviorChain(IServiceCollection services, Type featureType, Type resultType, Type handlerType)
{
    internal readonly IServiceCollection Services = services;
    internal readonly Type FeatureType = featureType;
    internal readonly Type ResultType = resultType;
    internal readonly Type HandlerType = handlerType;
    internal readonly List<Type> Behaviors = [];


    /// <summary>
    /// Adds a custom behavior
    /// </summary>
    public BehaviorChain Add(Type type)
    {
        var pipType        = typeof(IPipelineBehavior<,>);
        var implementsType = type.GetInterfaces()
                                 .Any(@interface => @interface.GetGenericTypeDefinition() == pipType);

        if (implementsType is false)
        {
            throw new InvalidOperationException($"{type.FullName} does not implement {pipType.FullName}");
        }

        Behaviors.Add(type.MakeGenericType(FeatureType, ResultType));
        Services.TryAddTransient(type);

        return this;
    }

    /// <summary>
    /// Adds a custom behavior, in a closed-generic way
    /// </summary>
    public BehaviorChain AddConcrete(Type type)
    {
        var pipType = typeof(IPipelineBehavior<,>)
            .MakeGenericType(FeatureType, ResultType);

        var implementsType = type.GetInterfaces().Any(@interface => @interface == pipType);

        if (implementsType is false)
        {
            throw new InvalidOperationException($"{type.FullName} does not implement {pipType.FullName}");
        }

        Behaviors.Add(type);
        Services.TryAddTransient(type);

        return this;
    }
}