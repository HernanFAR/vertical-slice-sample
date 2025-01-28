using FluentAssertions;
using FluentValidation;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Core;
using VSlices.Base.Definitions;
using VSlices.Core.Builder;
using VSlices.CrossCutting.Interceptor.FluentValidation;

namespace VSlices.CrossCutting.Pipeline.FluentValidation.UnitTests.Extensions;

public class FluentValidationBehaviorExtensionsTests
{
    public record Result;
    public record Input;

    public class Behavior : IBehavior<Input, Result>
    {
        public Eff<VSlicesRuntime, Result> Define(Input input) => throw new NotImplementedException();
    }

    public class Validator : AbstractValidator<Input>;

    [Fact]
    public void AddFluentValidationPipeline_ShouldRegisterInServiceContainer()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        InterceptorChain<Input, Result, Behavior> chain = new(services);

        // Act
        chain.AddValidation().UsingFluent<Validator>();

        // Assert
        services.Where(e => e.ServiceType == typeof(FluentValidationInterceptor<,>))
                .Any(e => e.Lifetime      == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType        == typeof(IValidator<Input>))
                .Where(e => e.ImplementationType == typeof(Validator))
                .Any(e => e.Lifetime             == ServiceLifetime.Transient)
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(FluentValidationInterceptor<Input, Result>));

    }
}
