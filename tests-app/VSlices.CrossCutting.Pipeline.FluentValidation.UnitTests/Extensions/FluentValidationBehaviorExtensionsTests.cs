using FluentAssertions;
using FluentValidation;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Core.Builder;

namespace VSlices.CrossCutting.Pipeline.FluentValidation.UnitTests.Extensions;

public class FluentValidationBehaviorExtensionsTests
{
    public record Result;
    public record Request : IFeature<Result>;

    public class Handler : IHandler<Request, Result>
    {
        public Eff<VSlicesRuntime, Result> Define(Request input)
        {
            throw new NotImplementedException();
        }
    }

    public class Validator : AbstractValidator<Request>;

    [Fact]
    public void AddFluentValidationPipeline_ShouldRegisterInServiceContainer()
    {
        BehaviorChain definition = new(new ServiceCollection(), typeof(Request), typeof(Result), typeof(Handler));

        definition.AddFluentValidationUsing<Validator>();

        definition.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(FluentValidationBehavior<Request, Result>))
            .Should().BeTrue();

        definition.Services
            .Where(e => e.ServiceType == typeof(IValidator<Request>))
            .Any(e => e.ImplementationType == typeof(Validator))
            .Should().BeTrue();
    }
}
