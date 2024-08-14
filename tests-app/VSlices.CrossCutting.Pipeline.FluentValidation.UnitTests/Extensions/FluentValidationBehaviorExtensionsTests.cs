using FluentAssertions;
using FluentValidation;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Core.Builder;

namespace VSlices.CrossCutting.Pipeline.FluentValidation.UnitTests.Extensions;

public class FluentValidationBehaviorExtensionsTests
{
    public record Result;
    public record Request : IFeature<Result>;

    public class Handler : IHandler<Request, Result>
    {
        public Eff<VSlicesRuntime, Result> Define(Request input) => throw new NotImplementedException();
    }

    public class Validator : AbstractValidator<Request>;

    [Fact]
    public void AddFluentValidationPipeline_ShouldRegisterInServiceContainer()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        BehaviorChain chain = new(services, typeof(Request), typeof(Result), typeof(Handler));

        // Act
        chain.AddFluentValidationUsing<Validator>();

        // Assert
        services.Where(e => e.ServiceType == typeof(FluentValidationBehavior<,>))
                .Any(e => e.Lifetime      == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType        == typeof(IValidator<Request>))
                .Where(e => e.ImplementationType == typeof(Validator))
                .Any(e => e.Lifetime             == ServiceLifetime.Transient)
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(FluentValidationBehavior<Request, Result>));

    }
}
