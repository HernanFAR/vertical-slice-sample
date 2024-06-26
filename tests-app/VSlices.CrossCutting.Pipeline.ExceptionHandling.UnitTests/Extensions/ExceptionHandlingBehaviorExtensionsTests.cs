using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using LanguageExt.SysX.Live;
using VSlices.Base;
using VSlices.Core.Builder;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling.UnitTests.Extensions;

public class ExceptionHandlingBehaviorExtensionsTests
{
    public record FalsePipeline : IPipelineBehavior;

    public record Result;

    public record Request : IFeature<Result>;
    
    public class TestPipeline1<TRequest, TResult> : AbstractExceptionHandlingBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        protected internal override Aff<Runtime, TResult> Process(Exception ex, TRequest request)
        {
            throw new UnreachableException();
        }
    }
    
    public class TestPipeline2<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        public Aff<Runtime, TResult> Define(TRequest request, Aff<Runtime, TResult> next)
        {
            throw new UnreachableException();
        }
    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldRegisterInServiceContainer()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddExceptionHandlingBehavior<TestPipeline1<Request, Result>>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(TestPipeline1<Request, Result>))
            .Should().BeTrue();

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldThrowInvalidOperation_DetailDoesNotImplementPipelineBehavior()
    {
        var expMessage = $"The type {typeof(object).FullName} does not implement {typeof(IPipelineBehavior<,>).FullName}";
        
        FeatureBuilder builder = new(new ServiceCollection());

        Func<FeatureBuilder> act = () => builder.AddExceptionHandlingBehavior<FalsePipeline>();

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldThrowInvalidOperation_DetailDoesNotImplementExceptionHandlingBehavior()
    {
        var expMessage = $"Type {typeof(TestPipeline2<Request, Result>).FullName} must inherit from {typeof(AbstractExceptionHandlingBehavior<,>).FullName}";

        FeatureBuilder builder = new(new ServiceCollection());

        Func<FeatureBuilder> act = () => builder.AddExceptionHandlingBehavior<TestPipeline2<Request, Result>>();

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }
}
