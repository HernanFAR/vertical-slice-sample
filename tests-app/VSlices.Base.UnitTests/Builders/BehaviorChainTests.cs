using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Base.Definitions;

namespace VSlices.Base.UnitTests.Builders;

public sealed class InterceptorChainTests
{
    public sealed record Result;

    public sealed record Input;

    public sealed class Behavior : IBehavior<Input, Result>
    {
        public Eff<VSlicesRuntime, Result> Define(Input input) => throw new NotImplementedException();
    }

    public sealed class Pipeline<TRequest, TResult> : IBehaviorInterceptor<TRequest, TResult>
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

        InterceptorChain<Input, Result, Behavior> chain = new(services);

        // Act
        chain.Add(typeof(Pipeline<,>));

        // Assert
        services.Count.Should().Be(expServiceCount);
        services.Single().ServiceType.Should().Be(typeof(Pipeline<,>));

        chain.Behaviors.Count.Should().Be(expBehaviorCount);
        chain.Behaviors.Single().Should().Be(typeof(Pipeline<Input, Result>));

    }

    [Fact]
    public void Add_Success_ShouldAddConcrete()
    {
        // Arrange
        const int expServiceCount  = 1;
        const int expBehaviorCount = 1;
        var       services         = new ServiceCollection();

        InterceptorChain<Input, Result, Behavior> chain = new(services);

        // Act
        chain.AddConcrete(typeof(Pipeline<Input, Result>));

        // Assert
        services.Count.Should().Be(expServiceCount);
        services.Single().ServiceType.Should().Be(typeof(Pipeline<Input, Result>));

        chain.Behaviors.Count.Should().Be(expBehaviorCount);
        chain.Behaviors.Single().Should().Be(typeof(Pipeline<Input, Result>));

    }
}
