using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System.Diagnostics;
using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.Builder;
using VSlices.CrossCutting.Pipeline;
using static VSlices.CrossCutting.UnitTests.Extensions.FeatureBuilderExtensions;

namespace VSlices.CrossCutting.UnitTests.Extensions;

public class FeatureBuilderExtensions
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
