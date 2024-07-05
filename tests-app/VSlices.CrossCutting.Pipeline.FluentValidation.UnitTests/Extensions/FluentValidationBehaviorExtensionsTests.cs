using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
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
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddFluentValidationUsing<Validator>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, RequestResult>))
            .Any(e => e.ImplementationType == typeof(FluentValidationBehavior<Request, RequestResult>))
            .Should().BeTrue();

        builder.Services
            .Where(e => e.ServiceType == typeof(IValidator<Request>))
            .Any(e => e.ImplementationType == typeof(Validator))
            .Should().BeTrue();
    }
}
