using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core;
using VSlices.Core.Builder;
using VSlices.Core.Stream;
using VSlices.CrossCutting.StreamPipeline;

namespace VSlices.CrossCutting.Pipeline.UnitTests.Extensions;

public class FeatureBuilderExtensionsTests
{
    public record Result;

    public record Request : IStream<Result>;

    public class TestPipeline<TRequest, TResult> : IStreamPipelineBehavior<TRequest, TResult>
        where TRequest : IStream<TResult>
    {
        public Eff<HandlerRuntime, IAsyncEnumerable<TResult>> Define(TRequest request, Eff<HandlerRuntime, IAsyncEnumerable<TResult>> next)
        {
            return next;
        }
    }

    [Fact]
    public void AddPipeline_ShouldRegisterInServiceContainer()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddStreamPipeline<TestPipeline<Request, Result>>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IStreamPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(TestPipeline<Request, Result>))
            .Should().BeTrue();

    }
}
