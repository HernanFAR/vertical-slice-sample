using FluentAssertions;
using LanguageExt;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Core.Builder;

namespace VSlices.CrossCutting.Pipeline.UnitTests.Extensions;

public class FeatureBuilderExtensionsTests
{
    public record Result;

    public record Request : IFeature<Result>;

    public class TestPipeline<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        public Aff<Runtime, TResult> Define(TRequest request, Aff<Runtime, TResult> next)
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
