using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Core;

namespace VSlices.Base.Builder;

/// <summary>
/// Specifies a fluent api to define dependencies of a non specified <see cref="IFeature{TResult}" />
/// </summary>
public sealed class FeatureDefinition<TFeature, TResult>(IServiceCollection services) 
    : IFeatureStartBuilder<TFeature, TResult>,
      IFeatureIntegratorBuilder<TFeature, TResult>,
      IFeatureOtherIntegratorsBuilder<TFeature, TResult>
    where TFeature : IFeature<TResult>
{
    internal readonly IServiceCollection Services = services;

    /// <inheritdoc />
    public IFeatureIntegratorBuilder<TFeature, TResult> FromIntegration => this;

    /// <inheritdoc />
    public IFeatureHandlerBuilder<TFeature, TResult, THandler> Execute<THandler>()
        where THandler : class, IHandler<TFeature, TResult>
    {
        Services.AddTransient<IHandler<TFeature, TResult>, THandler>();

        return new FeatureDefinition<TFeature, TResult, THandler>(Services);
    }

    /// <inheritdoc />
    public IFeatureOtherIntegratorsBuilder<TFeature, TResult> And<TIntegrator>()
        where TIntegrator : class, IIntegrator
    {
        Services.AddSingleton<IIntegrator, TIntegrator>();

        return this;
    }

    /// <inheritdoc />
    public IFeatureOtherIntegratorsBuilder<TFeature, TResult> Using<TIntegrator>()
        where TIntegrator : class, IIntegrator
    {
        Services.AddSingleton<IIntegrator, TIntegrator>();

        return this;
    }
}

/// <summary>
/// Specifies a fluent api to define dependencies of a non specified <see cref="IFeature{TResult}" />
/// </summary>
public sealed class FeatureDefinition<TFeature, TResult, THandler>(IServiceCollection services)
    : IFeatureHandlerBuilder<TFeature, TResult, THandler>
    where TFeature : IFeature<TResult> 
    where THandler : class, IHandler<TFeature, TResult>
{
    internal readonly IServiceCollection Services = services;

    /// <inheritdoc />
    public void WithBehaviorChain(BehaviorChainConfigAction chain)
    {
        BehaviorChain order = new(Services, typeof(TFeature), typeof(TResult), typeof(THandler));

        chain(order);
        
        Services.AddSingleton(new HandlerBehaviorChain<THandler>(order.Behaviors));
    } 
}

/// <summary>
/// Specifies a fluent api to define dependencies of a specified <see cref="IFeature{TResult}" />
/// </summary>
public interface IFeatureStartBuilder<TFeature, TResult>
    where TFeature : IFeature<TResult>
{
    /// <summary>
    /// The feature will be executed from integrations defined in the next step.
    /// </summary>
    IFeatureIntegratorBuilder<TFeature, TResult> FromIntegration { get; }

    /// <summary>
    /// The feature is associated with this handler
    /// </summary>
    public IFeatureHandlerBuilder<TFeature, TResult, THandler> Execute<THandler>()
        where THandler : class, IHandler<TFeature, TResult>;
}

/// <summary>
/// Specifies a fluent api to define dependencies of a specified <see cref="IFeature{TResult}" />
/// </summary>
public interface IFeatureIntegratorBuilder<TFeature, TResult>
    where TFeature : IFeature<TResult>
{
    /// <summary>
    /// The feature is executed in this integration
    /// </summary>
    IFeatureOtherIntegratorsBuilder<TFeature, TResult> Using<TIntegrator>() 
        where TIntegrator : class, IIntegrator;
    
}

/// <summary>
/// Specifies a fluent api to define dependencies of a specified <see cref="IFeature{TResult}" />
/// </summary>
public interface IFeatureOtherIntegratorsBuilder<TFeature, TResult>
    where TFeature : IFeature<TResult>
{
    /// <summary>
    /// The feature is executed in this integration
    /// </summary>
    IFeatureOtherIntegratorsBuilder<TFeature, TResult> And<TIntegrator>()
        where TIntegrator : class, IIntegrator;

    /// <summary>
    /// The feature is associated with this handler
    /// </summary>
    IFeatureHandlerBuilder<TFeature, TResult, THandler> Execute<THandler>()
        where THandler : class, IHandler<TFeature, TResult>;

}

/// <summary>
/// Specifies a fluent api to define dependencies of a specified <see cref="IFeature{TResult}" />
/// </summary>
public interface IFeatureHandlerBuilder<TFeature, TResult, THandler>
    where TFeature : IFeature<TResult>
    where THandler : class, IHandler<TFeature, TResult>
{
    /// <summary>
    /// Adds behaviors to the effect chain
    /// </summary>
    void WithBehaviorChain(BehaviorChainConfigAction chain);

}

/// <summary>
/// Define the behavior chain related to a concrete handler type
/// </summary>
public delegate void BehaviorChainConfigAction(BehaviorChain chain);