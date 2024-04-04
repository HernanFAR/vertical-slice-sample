using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.Builder;

namespace VSlices.CrossCutting.Pipeline.UnitTests.Extensions;

public class FeatureBuilderExtensionsTests
{
    public record RequestResult;
    public record Request : IFeature<RequestResult>;
    public class TestPipeline<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        public ValueTask<Result<TResult>> HandleAsync(TRequest request,
            RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
        {
            throw new UnreachableException();
        }
    }

    [Fact]
    public void AddPipeline_ShouldRegisterInServiceContainer()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddPipeline<TestPipeline<Request, RequestResult>>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, RequestResult>))
            .Any(e => e.ImplementationType == typeof(TestPipeline<Request, RequestResult>))
            .Should().BeTrue();

    }
}
