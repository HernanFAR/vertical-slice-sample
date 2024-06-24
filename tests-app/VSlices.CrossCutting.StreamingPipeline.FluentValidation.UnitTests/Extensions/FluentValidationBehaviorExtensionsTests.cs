using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Core.Builder;
using VSlices.Core.Stream;
using VSlices.CrossCutting.StreamPipeline;
using VSlices.CrossCutting.StreamPipeline.FluentValidation;

namespace VSlices.CrossCutting.Pipeline.FluentValidation.UnitTests.Extensions;

public class FluentValidationBehaviorExtensionsTests
{
    public record Result;
    
    public record Request : IStream<Result>;

    public class Validator : AbstractValidator<Request> { }

    [Fact]
    public void AddFluentValidationPipeline_ShouldRegisterInServiceContainer()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddFluentValidationStreamBehavior<Validator>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IStreamPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(FluentValidationStreamBehavior<Request, Result>))
            .Should().BeTrue();

        builder.Services
            .Where(e => e.ServiceType == typeof(IValidator<Request>))
            .Any(e => e.ImplementationType == typeof(Validator))
            .Should().BeTrue();
    }
}
