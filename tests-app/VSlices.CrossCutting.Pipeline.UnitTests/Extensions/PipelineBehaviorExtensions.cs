using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Core;
using VSlices.Core.Builder;

namespace VSlices.CrossCutting.Pipeline.UnitTests.Extensions;

public class FeatureBuilderExtensionsTests
{
    public record Result;

    public record Request : IFeature<Result>;

    public class TestPipeline<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        public Eff<HandlerRuntime, TResult> Define(TRequest request, Eff<HandlerRuntime, TResult> next)
        {
            return next;
        }
    }

    [Fact]
    public void AddPipeline_ShouldRegisterInServiceContainer()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddPipeline<TestPipeline<Request, Result>>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(TestPipeline<Request, Result>))
            .Should().BeTrue();

    }
}
