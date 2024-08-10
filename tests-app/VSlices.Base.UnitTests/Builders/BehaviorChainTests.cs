using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;

namespace VSlices.Base.UnitTests.Builders;

public sealed class BehaviorChainTests
{
    public sealed record Result;

    public sealed record Feature : IFeature<Result>;

    public sealed class Handler : IHandler<Feature, Result>
    {
        public Eff<VSlicesRuntime, Result> Define(Feature input) => throw new NotImplementedException();
    }

    public sealed class Pipeline<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        public Eff<VSlicesRuntime, TResult> Define(TRequest request, Eff<VSlicesRuntime, TResult> next) => throw new NotImplementedException();
    }

    [Fact]
    public void Add_Success_ShouldAdd()
    {
        // Arrange
        const int expServiceCount  = 1;
        const int expBehaviorCount = 1;
        var       services         = new ServiceCollection();

        BehaviorChain chain = new(services, typeof(Feature), typeof(Result), typeof(Handler));

        // Act
        chain.Add(typeof(Pipeline<,>));

        // Assert
        services.Count.Should().Be(expServiceCount);
        services.Single().ServiceType.Should().Be(typeof(Pipeline<,>));

        chain.Behaviors.Count.Should().Be(expBehaviorCount);
        chain.Behaviors.Single().Should().Be(typeof(Pipeline<Feature, Result>));

    }

    [Fact]
    public void Add_Success_ShouldAddConcrete()
    {
        // Arrange
        const int expServiceCount  = 1;
        const int expBehaviorCount = 1;
        var       services         = new ServiceCollection();

        BehaviorChain chain = new(services, typeof(Feature), typeof(Result), typeof(Handler));

        // Act
        chain.AddConcrete(typeof(Pipeline<Feature, Result>));

        // Assert
        services.Count.Should().Be(expServiceCount);
        services.Single().ServiceType.Should().Be(typeof(Pipeline<Feature, Result>));

        chain.Behaviors.Count.Should().Be(expBehaviorCount);
        chain.Behaviors.Single().Should().Be(typeof(Pipeline<Feature, Result>));

    }
}
