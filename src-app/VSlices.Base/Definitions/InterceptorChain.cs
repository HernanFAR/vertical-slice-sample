using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;

namespace VSlices.Base.Definitions;

/// <summary>
/// Define the behavior chain related to a concrete handler type
/// </summary>
public sealed class InterceptorChain<TIn, TOut, TBehavior>(IServiceCollection services)
    where TBehavior : IBehavior<TIn, TOut>
{
    /// <summary>
    /// Service collection in use
    /// </summary>
    public IServiceCollection Services { get; } = services;

    internal readonly List<Type> Behaviors = [];


    /// <summary>
    /// Adds a custom behavior
    /// </summary>
    public InterceptorChain<TIn, TOut, TBehavior> Add(Type type)
    {
        var pipType = typeof(IBehaviorInterceptor<,>);
        var implementsType = type.GetInterfaces()
                                 .Any(@interface => @interface.GetGenericTypeDefinition() == pipType);

        if (implementsType is false)
        {
            throw new InvalidOperationException($"{type.FullName} does not implement {pipType.FullName}");
        }

        Behaviors.Add(type.MakeGenericType(typeof(TIn), typeof(TOut)));
        Services.TryAddTransient(type);

        return this;
    }

    /// <summary>
    /// Adds a custom behavior, in a closed-generic way
    /// </summary>
    public InterceptorChain<TIn, TOut, TBehavior> AddConcrete(Type type)
    {
        var pipType = typeof(IBehaviorInterceptor<,>)
            .MakeGenericType(typeof(TIn), typeof(TOut));

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