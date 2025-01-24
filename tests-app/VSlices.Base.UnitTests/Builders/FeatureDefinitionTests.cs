using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Base.Definitions;

namespace VSlices.Base.UnitTests.Builders;

public sealed class FeatureComposerTests
{
    public sealed record Result;

    public sealed record Input;

    public sealed class Behavior : IBehavior<Input, Result>
    {
        public Eff<VSlicesRuntime, Result> Define(Input input) => throw new NotImplementedException();
    }

    public sealed class BehaviorInterceptor<TFeature, TResult> : IBehaviorInterceptor<TFeature, TResult>
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

        var feature = new FeatureComposer(services).With<Input>().Expect<Result>();

        // Act
        feature.ByExecuting<Behavior>();

        // Assert
        services.Count.Should().Be(expServiceCount);

        var descriptor = services.Single(x => x.ImplementationType == typeof(Behavior));

        descriptor.ServiceType.Should().Be(typeof(IBehavior<Input, Result>));
    }

    [Fact]
    public void FeatureDefinition_DefineFeatureDependencies_WithoutIntegrations()
    {
        // Arrange
        const int expServiceCount = 3;

        var services = new ServiceCollection();

        var feature = new FeatureComposer(services).With<Input>().Expect<Result>();

        // Act
        feature.ByExecuting<Behavior>(chain => chain.Add(typeof(BehaviorInterceptor<,>)));

        // Assert
        services.Count.Should().Be(expServiceCount);

        var handlerDescriptor = services.Single(x => x.ImplementationType == typeof(Behavior));
        handlerDescriptor.ServiceType.Should().Be(typeof(IBehavior<Input, Result>));

        var behaviorDescriptor = services.Single(x => x.ImplementationType == typeof(BehaviorInterceptor<,>));
        behaviorDescriptor.ServiceType.Should().Be(typeof(BehaviorInterceptor<,>));

        var behaviorChainDescriptor = services.Single(x => x.ServiceType == typeof(BehaviorInterceptorChain<Behavior>));
        behaviorChainDescriptor.ImplementationType.Should().BeNull();
    }

    [Fact]
    public void FeatureDefinition_DefineFeatureDependencies_WithoutBehaviors()
    {
        // Arrange
        const int expServiceCount = 2;

        var services = new ServiceCollection();

        var feature = new FeatureComposer(services).With<Input>().Expect<Result>();

        // Act
        feature.ByExecuting<Behavior>()
               .AndBindTo<Integrator>();

        // Assert
        services.Count.Should().Be(expServiceCount);

        var presentationDescriptor = services.Single(x => x.ImplementationType == typeof(Integrator));
        presentationDescriptor.ServiceType.Should().Be(typeof(IIntegrator));    

        var handlerDescriptor = services.Single(x => x.ImplementationType == typeof(Behavior));
        handlerDescriptor.ServiceType.Should().Be(typeof(IBehavior<Input, Result>));

    }

    [Fact]
    public void FeatureDefinition_DefineFeatureDependencies()
    {
        // Arrange
        const int expServiceCount = 4;

        var services = new ServiceCollection();

        var feature = new FeatureComposer(services).With<Input>().Expect<Result>();

        // Act
        feature.ByExecuting<Behavior>(chain => chain.Add(typeof(BehaviorInterceptor<,>)))
               .AndBindTo<Integrator>();

        // Assert
        services.Count.Should().Be(expServiceCount);

        var presentationDescriptor = services.Single(x => x.ImplementationType == typeof(Integrator));
        presentationDescriptor.ServiceType.Should().Be(typeof(IIntegrator));

        var handlerDescriptor = services.Single(x => x.ImplementationType == typeof(Behavior));
        handlerDescriptor.ServiceType.Should().Be(typeof(IBehavior<Input, Result>));

        var behaviorDescriptor = services.Single(x => x.ImplementationType == typeof(BehaviorInterceptor<,>));
        behaviorDescriptor.ServiceType.Should().Be(typeof(BehaviorInterceptor<,>));

        var behaviorChainDescriptor = services.Single(x => x.ServiceType == typeof(BehaviorInterceptorChain<Behavior>));
        behaviorChainDescriptor.ImplementationType.Should().BeNull();

    }

    [Fact]
    public void FeatureDefinition_DefineFeatureDependencies_WithTwoIntegrations()
    {
        // Arrange
        const int expServiceCount = 5;

        var services = new ServiceCollection();

        var feature = new FeatureComposer(services).With<Input>().Expect<Result>();

        // Act
        feature.ByExecuting<Behavior>(chain => chain.Add(typeof(BehaviorInterceptor<,>)))
               .AndBindTo<Integrator, OtherIntegrator>();

        // Assert
        services.Count.Should().Be(expServiceCount);

        var presentationDescriptor = services.Single(x => x.ImplementationType == typeof(Integrator));
        presentationDescriptor.ServiceType.Should().Be(typeof(IIntegrator));

        var presentationDescriptor2 = services.Single(x => x.ImplementationType == typeof(OtherIntegrator));
        presentationDescriptor2.ServiceType.Should().Be(typeof(IIntegrator));

        var handlerDescriptor = services.Single(x => x.ImplementationType == typeof(Behavior));
        handlerDescriptor.ServiceType.Should().Be(typeof(IBehavior<Input, Result>));

        var behaviorDescriptor = services.Single(x => x.ImplementationType == typeof(BehaviorInterceptor<,>));
        behaviorDescriptor.ServiceType.Should().Be(typeof(BehaviorInterceptor<,>));

        var behaviorChainDescriptor = services.Single(x => x.ServiceType == typeof(BehaviorInterceptorChain<Behavior>));
        behaviorChainDescriptor.ImplementationType.Should().BeNull();

    }
}
