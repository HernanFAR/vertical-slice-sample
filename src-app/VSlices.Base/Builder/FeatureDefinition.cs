﻿using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Core;

namespace VSlices.Base.Builder;

/// <summary>
/// Specifies a fluent api to define dependencies of a non specified <see cref="IFeature{TResult}" />
/// </summary>
public sealed class FeatureDefinition<TFeature, TResult>(IServiceCollection services) 
    : IFeatureStartBuilder<TFeature, TResult>,
      IFeaturePresentationBuilder<TFeature, TResult>,
      IFeatureOtherPresentationsBuilder<TFeature, TResult>
    where TFeature : IFeature<TResult>
{
    internal readonly IServiceCollection Services = services;

    /// <inheritdoc />
    public IFeaturePresentationBuilder<TFeature, TResult> FromIntegration => this;

    /// <inheritdoc />
    public IFeatureHandlerBuilder<TFeature, TResult, THandler> ByExecuting<THandler>()
        where THandler : class, IHandler<TFeature, TResult>
    {
        Services.AddTransient<IHandler<TFeature, TResult>, THandler>();

        return new FeatureDefinition<TFeature, TResult, THandler>(Services);
    }

    /// <inheritdoc />
    public IFeaturePresentationBuilder<TFeature, TResult> And<TPresentation>()
        where TPresentation : class, IPresentationDefinition
    {
        Services.AddSingleton<IPresentationDefinition, TPresentation>();

        return this;
    }

    /// <inheritdoc />
    public IFeatureOtherPresentationsBuilder<TFeature, TResult> With<TPresentation>()
        where TPresentation : class, IPresentationDefinition
    {
        Services.AddSingleton<IPresentationDefinition, TPresentation>();

        return this;
    }

    /// <inheritdoc />
    public IFeatureHandlerBuilder<TFeature, TResult, THandler> Executing<THandler>()
        where THandler : class, IHandler<TFeature, TResult>
    {
        Services.AddTransient<IHandler<TFeature, TResult>, THandler>();

        return new FeatureDefinition<TFeature, TResult, THandler>(Services);
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
    public void AddBehaviors(Action<BehaviorChain> chain)
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
    IFeaturePresentationBuilder<TFeature, TResult> FromIntegration { get; }

    /// <summary>
    /// The feature is associated with this handler
    /// </summary>
    public IFeatureHandlerBuilder<TFeature, TResult, THandler> ByExecuting<THandler>()
        where THandler : class, IHandler<TFeature, TResult>;
}

/// <summary>
/// Specifies a fluent api to define dependencies of a specified <see cref="IFeature{TResult}" />
/// </summary>
public interface IFeaturePresentationBuilder<TFeature, TResult>
    where TFeature : IFeature<TResult>
{
    /// <summary>
    /// The feature is executed in this integration
    /// </summary>
    IFeatureOtherPresentationsBuilder<TFeature, TResult> With<TPresentation>() 
        where TPresentation : class, IPresentationDefinition;
    
}

/// <summary>
/// Specifies a fluent api to define dependencies of a specified <see cref="IFeature{TResult}" />
/// </summary>
public interface IFeatureOtherPresentationsBuilder<TFeature, TResult>
    where TFeature : IFeature<TResult>
{
    /// <summary>
    /// The feature is executed in this integration
    /// </summary>
    IFeaturePresentationBuilder<TFeature, TResult> And<TPresentation>()
        where TPresentation : class, IPresentationDefinition;

    /// <summary>
    /// The feature is associated with this handler
    /// </summary>
    IFeatureHandlerBuilder<TFeature, TResult, THandler> Executing<THandler>()
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
    void AddBehaviors(Action<BehaviorChain> chain);

}