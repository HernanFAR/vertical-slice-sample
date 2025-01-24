using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base.CrossCutting;

namespace VSlices.Base.Definitions;

/// <summary>
/// Define the behavior chain related to a concrete handler type
/// </summary>
public sealed class InterceptorChain(IServiceCollection services, Type inType, Type outType, Type behaviorType)
{
    internal readonly IServiceCollection Services = services;
    internal readonly Type InType = inType;
    internal readonly Type OutType = outType;
    internal readonly Type BehaviorType = behaviorType;
    internal readonly List<Type> Behaviors = [];


    /// <summary>
    /// Adds a custom behavior
    /// </summary>
    public InterceptorChain Add(Type type)
    {
        var pipType = typeof(IBehaviorInterceptor<,>);
        var implementsType = type.GetInterfaces()
                                 .Any(@interface => @interface.GetGenericTypeDefinition() == pipType);

        if (implementsType is false)
        {
            throw new InvalidOperationException($"{type.FullName} does not implement {pipType.FullName}");
        }

        Behaviors.Add(type.MakeGenericType(InType, OutType));
        Services.TryAddTransient(type);

        return this;
    }

    /// <summary>
    /// Adds a custom behavior, in a closed-generic way
    /// </summary>
    public InterceptorChain AddConcrete(Type type)
    {
        var pipType = typeof(IBehaviorInterceptor<,>)
            .MakeGenericType(InType, OutType);

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