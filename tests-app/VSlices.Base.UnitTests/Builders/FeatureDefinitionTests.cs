using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;

namespace VSlices.Base.UnitTests.Builders;

public sealed class FeatureDefinitionTests
{
    public sealed record Result;

    public sealed record Feature : IFeature<Result>;

    public sealed class Handler : IHandler<Feature, Result>
    {
        public Eff<VSlicesRuntime, Result> Define(Feature input) => throw new NotImplementedException();
    }

    public sealed class PipelineBehavior<TFeature, TResult> : IPipelineBehavior<TFeature, TResult>
        where TFeature : IFeature<TResult>
    {
        public Eff<VSlicesRuntime, TResult> Define(TFeature request, Eff<VSlicesRuntime, TResult> next) => throw new NotImplementedException();
    }

    public sealed class Integrator : IIntegrator;

    public sealed class OtherIntegrator : IIntegrator;

    [Fact]
    public void FeatureDefinition_DefineFeatureDependencies_WithoutIntegrationsNeitherBehaviors()
    {
        // Arrange
        const int expServiceCount = 1;

        var services = new ServiceCollection();

        IFeatureStartBuilder<Feature, Result> feature = new FeatureDefinition<Feature, Result>(services);

        // Act
        feature.Execute<Handler>();

        // Assert
        services.Count.Should().Be(expServiceCount);

        var descriptor = services.Single(x => x.ImplementationType == typeof(Handler));

        descriptor.ServiceType.Should().Be(typeof(IHandler<Feature, Result>));
    }

    [Fact]
    public void FeatureDefinition_DefineFeatureDependencies_WithoutIntegrations()
    {
        // Arrange
        const int expServiceCount = 3;

        var services = new ServiceCollection();

        IFeatureStartBuilder<Feature, Result> feature = new FeatureDefinition<Feature, Result>(services);

        // Act
        feature.Execute<Handler>()
               .WithBehaviorChain(chain => chain.Add(typeof(PipelineBehavior<,>)));

        // Assert
        services.Count.Should().Be(expServiceCount);

        var handlerDescriptor = services.Single(x => x.ImplementationType == typeof(Handler));
        handlerDescriptor.ServiceType.Should().Be(typeof(IHandler<Feature, Result>));

        var behaviorDescriptor = services.Single(x => x.ImplementationType == typeof(PipelineBehavior<,>));
        behaviorDescriptor.ServiceType.Should().Be(typeof(PipelineBehavior<,>));

        var behaviorChainDescriptor = services.Single(x => x.ServiceType == typeof(HandlerBehaviorChain<Handler>));
        behaviorChainDescriptor.ImplementationType.Should().BeNull();
    }

    [Fact]
    public void FeatureDefinition_DefineFeatureDependencies_WithoutBehaviors()
    {
        // Arrange
        const int expServiceCount = 2;

        var services = new ServiceCollection();

        IFeatureStartBuilder<Feature, Result> feature = new FeatureDefinition<Feature, Result>(services);

        // Act
        feature.FromIntegration.Using<Integrator>()
               .Execute<Handler>();

        // Assert
        services.Count.Should().Be(expServiceCount);

        var presentationDescriptor = services.Single(x => x.ImplementationType == typeof(Integrator));
        presentationDescriptor.ServiceType.Should().Be(typeof(IIntegrator));    

        var handlerDescriptor = services.Single(x => x.ImplementationType == typeof(Handler));
        handlerDescriptor.ServiceType.Should().Be(typeof(IHandler<Feature, Result>));

    }

    [Fact]
    public void FeatureDefinition_DefineFeatureDependencies()
    {
        // Arrange
        const int expServiceCount = 4;

        var services = new ServiceCollection();

        IFeatureStartBuilder<Feature, Result> feature = new FeatureDefinition<Feature, Result>(services);

        // Act
        feature.FromIntegration.Using<Integrator>()
               .Execute<Handler>()
               .WithBehaviorChain(chain => chain.Add(typeof(PipelineBehavior<,>)));

        // Assert
        services.Count.Should().Be(expServiceCount);

        var presentationDescriptor = services.Single(x => x.ImplementationType == typeof(Integrator));
        presentationDescriptor.ServiceType.Should().Be(typeof(IIntegrator));

        var handlerDescriptor = services.Single(x => x.ImplementationType == typeof(Handler));
        handlerDescriptor.ServiceType.Should().Be(typeof(IHandler<Feature, Result>));

        var behaviorDescriptor = services.Single(x => x.ImplementationType == typeof(PipelineBehavior<,>));
        behaviorDescriptor.ServiceType.Should().Be(typeof(PipelineBehavior<,>));

        var behaviorChainDescriptor = services.Single(x => x.ServiceType == typeof(HandlerBehaviorChain<Handler>));
        behaviorChainDescriptor.ImplementationType.Should().BeNull();

    }

    [Fact]
    public void FeatureDefinition_DefineFeatureDependencies_WithTwoIntegrations()
    {
        // Arrange
        const int expServiceCount = 5;

        var services = new ServiceCollection();

        IFeatureStartBuilder<Feature, Result> feature = new FeatureDefinition<Feature, Result>(services);

        // Act
        feature.FromIntegration.Using<Integrator>().And<OtherIntegrator>()
               .Execute<Handler>()
               .WithBehaviorChain(chain => chain.Add(typeof(PipelineBehavior<,>)));

        // Assert
        services.Count.Should().Be(expServiceCount);

        var presentationDescriptor = services.Single(x => x.ImplementationType == typeof(Integrator));
        presentationDescriptor.ServiceType.Should().Be(typeof(IIntegrator));

        var presentationDescriptor2 = services.Single(x => x.ImplementationType == typeof(OtherIntegrator));
        presentationDescriptor2.ServiceType.Should().Be(typeof(IIntegrator));

        var handlerDescriptor = services.Single(x => x.ImplementationType == typeof(Handler));
        handlerDescriptor.ServiceType.Should().Be(typeof(IHandler<Feature, Result>));

        var behaviorDescriptor = services.Single(x => x.ImplementationType == typeof(PipelineBehavior<,>));
        behaviorDescriptor.ServiceType.Should().Be(typeof(PipelineBehavior<,>));

        var behaviorChainDescriptor = services.Single(x => x.ServiceType == typeof(HandlerBehaviorChain<Handler>));
        behaviorChainDescriptor.ImplementationType.Should().BeNull();

    }
}
