using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using VSlices.Core.Builder;
using VSlices.Core.Stream;

namespace VSlices.CrossCutting.StreamPipeline.ExceptionHandling.UnitTests.Extensions;

public class ExceptionHandlingStreamBehaviorExtensionsTests
{
    public record Result;

    public record Request : IStream<Result>;
    
    public class TestPipeline1<TRequest, TResult> : AbstractExceptionHandlingStreamBehavior<TRequest, TResult>
        where TRequest : IStream<TResult>
    {
        protected internal override Aff<IAsyncEnumerable<TResult>> ProcessExceptionAsync(Exception ex, TRequest request, CancellationToken cancellationToken = default)
        {
            throw new UnreachableException();
        }
    }
    
    public class TestPipeline2<TRequest, TResult> : IStreamPipelineBehavior<TRequest, TResult>
        where TRequest : IStream<TResult>
    {
        public Aff<IAsyncEnumerable<TResult>> Define(TRequest request, Aff<IAsyncEnumerable<TResult>> next, CancellationToken cancellationToken)
        {
            throw new UnreachableException();
        }
    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldRegisterInServiceContainer()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddExceptionHandlingStreamPipeline<TestPipeline1<Request, Result>>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IStreamPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(TestPipeline1<Request, Result>))
            .Should().BeTrue();

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldThrowInvalidOperation_DetailDoesNotImplementPipelineBehavior()
    {
        var expMessage = $"The type {typeof(object).FullName} does not implement {typeof(IStreamPipelineBehavior<,>).FullName}";
        
        FeatureBuilder builder = new(new ServiceCollection());

        Func<FeatureBuilder> act = () => builder.AddExceptionHandlingStreamPipeline<object>();

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldThrowInvalidOperation_DetailDoesNotImplementExceptionHandlingBehavior()
    {
        var expMessage = $"Type {typeof(TestPipeline2<Request, Result>).FullName} must inherit from " +
                         $"{typeof(AbstractExceptionHandlingStreamBehavior<,>).FullName}";

        FeatureBuilder builder = new(new ServiceCollection());

        Func<FeatureBuilder> act = () => builder.AddExceptionHandlingStreamPipeline<TestPipeline2<Request, Result>>();

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }
}
