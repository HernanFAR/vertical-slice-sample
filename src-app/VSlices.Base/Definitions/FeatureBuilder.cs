using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;

namespace VSlices.Base.Definitions;

/// <summary>
/// Defines the input to use in this feature
/// </summary>
public sealed class FeatureComposer(IServiceCollection services)
{
    /// <summary>
    /// Sets <typeparamref name="TIn"/> as the input of the feature
    /// </summary>
    /// <typeparam name="TIn">The input <see cref="Type" /> to use</typeparam>
    /// <returns>A builder to set the output of the feature</returns>
    public FeatureOutput<TIn> With<TIn>() => new(services);
}

/// <summary>
/// Defines the output to use in this feature
/// </summary>
/// <typeparam name="TIn">The already set input <see cref="Type" /> to use</typeparam>
public sealed class FeatureOutput<TIn>(IServiceCollection services)
{
    /// <summary>
    /// Sets <typeparamref name="TOut"/> as the output of the feature, after setting <typeparamref name="TIn"/> as input.
    /// </summary>
    /// <typeparam name="TOut">The output <see cref="Type" /> to use</typeparam>
    /// <returns>A builder to set the <see cref="IBehavior{IBehaviorInterceptor,TOut}"/> of the feature</returns>
    public FeatureBehavior<TIn, TOut> Expect<TOut>() => new(services);

    /// <summary>
    /// Sets <see cref="Unit"/> as the output of the feature, after setting <typeparamref name="TIn"/> as input.
    /// </summary>
    /// <returns>A builder to set the <see cref="IBehavior{IBehaviorInterceptor,TOut}"/> of the feature</returns>
    public FeatureBehavior<TIn, Unit> ExpectNoOutput() => new(services);

}

/// <summary>
/// Defines the <see cref="IBehavior{IBehaviorInterceptor,TOut}"/> to use in this feature
/// </summary>
/// <typeparam name="TIn">The already set input <see cref="Type" /> to use</typeparam>
/// <typeparam name="TOut">The already set output <see cref="Type" /> to use</typeparam>
public sealed class FeatureBehavior<TIn, TOut>(IServiceCollection services)
{
    /// <summary>
    /// Sets <typeparamref name="TBehavior"/> to use.
    /// </summary>
    /// <typeparam name="TBehavior">The <see cref="IBehavior{IBehaviorInterceptor,TOut}" /> to use</typeparam>
    /// <returns>A builder to set the <see cref="IIntegrator"/> of the feature</returns>
    public FeatureIntegrator ByExecuting<TBehavior>()
        where TBehavior : class, IBehavior<TIn, TOut>
    {
        services.AddScoped<IBehavior<TIn, TOut>, TBehavior>();

        return new FeatureIntegrator(services);
    }

    /// <summary>
    /// Sets <typeparamref name="TBehavior"/> to use with a specific <see cref="IBehaviorInterceptor{TRequest,TResult}"/> chain.
    /// </summary>
    /// <typeparam name="TBehavior">The <see cref="IBehavior{IBehaviorInterceptor,TOut}" /> to use</typeparam>
    /// <returns>A builder to set the <see cref="IIntegrator"/> of the feature</returns>
    public FeatureIntegrator ByExecuting<TBehavior>(InterceptorChainConfigAction chain)
        where TBehavior : class, IBehavior<TIn, TOut>
    {
        services.AddScoped<IBehavior<TIn, TOut>, TBehavior>();

        InterceptorChain order = new(services, typeof(TIn), typeof(TOut), typeof(TBehavior));

        chain(order);

        services.AddSingleton(new BehaviorInterceptorChain<TBehavior>(order.Behaviors));

        return new FeatureIntegrator(services);
    }
}

/// <summary>
/// Defines the <see cref="IIntegrator"/> implementations of the features behavior
/// </summary>
/// <param name="services">Service collection</param>
public sealed class FeatureIntegrator(IServiceCollection services) 
{
    /// <summary>
    /// Specifies no binding to the behavior
    /// </summary>
    /// <returns>Unit</returns>
    public Unit AndNoBind() => unit;

    /// <summary>
    /// Specifies one bind to the behavior
    /// </summary>
    /// <returns>Unit</returns>
    public Unit AndBindTo<T>()
        where T : class, IIntegrator
    {
        services.AddSingleton<IIntegrator, T>();

        return unit;
    }

    /// <summary>
    /// Specifies two binds to the behavior
    /// </summary>
    /// <returns>Unit</returns>
    public Unit AndBindTo<T1, T2>()
        where T1 : class, IIntegrator
        where T2 : class, IIntegrator
    {
        services.AddSingleton<IIntegrator, T1>();
        services.AddSingleton<IIntegrator, T2>();

        return unit;
    }

    /// <summary>
    /// Specifies three binds to the behavior
    /// </summary>
    /// <returns>Unit</returns>
    public Unit AndBindTo<T1, T2, T3>()
        where T1 : class, IIntegrator
        where T2 : class, IIntegrator
        where T3 : class, IIntegrator
    {
        services.AddSingleton<IIntegrator, T1>();
        services.AddSingleton<IIntegrator, T2>();
        services.AddSingleton<IIntegrator, T3>();

        return unit;
    }
}

/// <summary>
/// Define the behavior chain related to a concrete handler type
/// </summary>
public delegate void InterceptorChainConfigAction(InterceptorChain chain);