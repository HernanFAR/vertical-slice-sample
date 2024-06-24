using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using VSlices.Base;
using VSlices.Core.Builder;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling.UnitTests.Extensions;

public class ExceptionHandlingBehaviorExtensionsTests
{
    public record Result;

    public record Request : IFeature<Result>;
    
    public class TestPipeline1<TRequest, TResult> : AbstractExceptionHandlingBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        protected internal override Aff<TResult> ProcessExceptionAsync(Exception ex, TRequest request, CancellationToken cancellationToken = default)
        {
            throw new UnreachableException();
        }
    }
    
    public class TestPipeline2<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        public Aff<TResult> Define(TRequest request, Aff<TResult> next, CancellationToken cancellationToken)
        {
            throw new UnreachableException();
        }
    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldRegisterInServiceContainer()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddExceptionHandlingPipeline<TestPipeline1<Request, Result>>();

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

        Func<FeatureBuilder> act = () => builder.AddExceptionHandlingPipeline<object>();

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldThrowInvalidOperation_DetailDoesNotImplementExceptionHandlingBehavior()
    {
        var expMessage = $"Type {typeof(TestPipeline2<Request, Result>).FullName} must inherit from {typeof(AbstractExceptionHandlingBehavior<,>).FullName}";

        FeatureBuilder builder = new(new ServiceCollection());

        Func<FeatureBuilder> act = () => builder.AddExceptionHandlingPipeline<TestPipeline2<Request, Result>>();

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }
}
