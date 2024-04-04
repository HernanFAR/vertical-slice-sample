using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.Builder;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling.UnitTests.Extensions;

public class ExceptionHandlingBehaviorExtensionsTests
{
    public record RequestResult;
    public record Request : IFeature<RequestResult>;
    public class TestPipeline1<TRequest, TResult> : AbstractExceptionHandlingBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        protected internal override ValueTask ProcessExceptionAsync(Exception ex, TRequest request)
        {
            throw new UnreachableException();
        }
    }
    public class TestPipeline2<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        public ValueTask<Result<TResult>> HandleAsync(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldRegisterInServiceContainer()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddExceptionHandlingPipeline<TestPipeline1<Request, RequestResult>>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, RequestResult>))
            .Any(e => e.ImplementationType == typeof(TestPipeline1<Request, RequestResult>))
            .Should().BeTrue();

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldThrowInvalidOperation_DetailDoesNotImplementPipelineBehavior()
    {
        var expMessage = $"The type {typeof(object).FullName} does not implement {typeof(IPipelineBehavior<,>).FullName}";
        
        FeatureBuilder builder = new(new ServiceCollection());

        var act = builder.AddExceptionHandlingPipeline<object>;

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldThrowInvalidOperation_DetailDoesNotImplementExceptionHandlingBehavior()
    {
        var expMessage = $"Type {typeof(TestPipeline2<Request, RequestResult>).FullName} must inherit from {typeof(AbstractExceptionHandlingBehavior<,>).FullName}";

        FeatureBuilder builder = new(new ServiceCollection());

        var act = builder.AddExceptionHandlingPipeline<TestPipeline2<Request, RequestResult>>;

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }
}
