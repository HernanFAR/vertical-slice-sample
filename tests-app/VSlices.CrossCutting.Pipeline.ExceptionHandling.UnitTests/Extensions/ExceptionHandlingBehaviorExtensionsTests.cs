using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using VSlices.Base;
using VSlices.Core.Builder;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling.UnitTests.Extensions;

public class ExceptionHandlingBehaviorExtensionsTests
{
    public record RequestResult;
    public record Request : IFeature<RequestResult>;
    public class TestPipeline<TRequest, TResult> : AbstractExceptionHandlingBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        protected internal override ValueTask ProcessExceptionAsync(Exception ex, TRequest request)
        {
            throw new UnreachableException();
        }
    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldRegisterInServiceContainer()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddExceptionHandlingPipeline<TestPipeline<Request, RequestResult>>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, RequestResult>))
            .Any(e => e.ImplementationType == typeof(TestPipeline<Request, RequestResult>))
            .Should().BeTrue();

    }
}
