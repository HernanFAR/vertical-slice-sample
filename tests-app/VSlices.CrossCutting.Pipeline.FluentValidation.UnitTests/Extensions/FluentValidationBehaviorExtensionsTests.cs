using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Core.Builder;

namespace VSlices.CrossCutting.Pipeline.FluentValidation.UnitTests.Extensions;

public class FluentValidationBehaviorExtensionsTests
{
    public record RequestResult;
    public record Request : IFeature<RequestResult>;
    public class Validator : AbstractValidator<Request> { }

    [Fact]
    public void AddFluentValidationPipeline_ShouldRegisterInServiceContainer()
    {
        FeatureDefinition<,> definition = new(new ServiceCollection());

        definition.AddFluentValidationBehaviorUsing<Validator>();

        definition.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, RequestResult>))
            .Any(e => e.ImplementationType == typeof(FluentValidationBehavior<Request, RequestResult>))
            .Should().BeTrue();

        definition.Services
            .Where(e => e.ServiceType == typeof(IValidator<Request>))
            .Any(e => e.ImplementationType == typeof(Validator))
            .Should().BeTrue();
    }
}
