using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Core;
using VSlices.Core.Builder;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling.UnitTests.Extensions;

public class ExceptionHandlingBehaviorExtensionsTests
{
    public record FalsePipeline : IPipelineBehavior;

    public record Result;

    public record Request : IFeature<Result>;
    
    public class TestPipeline1<TRequest, TResult> : ExceptionHandlingBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        protected internal override Eff<VSlicesRuntime, TResult> Process(Exception ex, TRequest request)
        {
            throw new UnreachableException();
        }
    }
    
    public class TestPipeline2<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
        where TRequest : IFeature<TResult>
    {
        public Eff<VSlicesRuntime, TResult> Define(TRequest request, Eff<VSlicesRuntime, TResult> next)
        {
            throw new UnreachableException();
        }
    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldRegisterInServiceContainer()
    {
        FeatureDefinition<,> definition = new(new ServiceCollection());

        definition.AddExceptionHandlingBehavior<TestPipeline1<Request, Result>>();

        definition.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(TestPipeline1<Request, Result>))
            .Should().BeTrue();

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldThrowInvalidOperation_DetailDoesNotImplementPipelineBehavior()
    {
        var expMessage = $"The type {typeof(FalsePipeline).FullName} does not implement {typeof(IPipelineBehavior<,>).FullName}";
        
        FeatureDefinition<,> definition = new(new ServiceCollection());

        Func<FeatureDefinition<,>> act = () => definition.AddExceptionHandlingBehavior<FalsePipeline>();

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldThrowInvalidOperation_DetailDoesNotImplementExceptionHandlingBehavior()
    {
        var expMessage = $"Type {typeof(TestPipeline2<Request, Result>).FullName} must inherit from {typeof(ExceptionHandlingBehavior<,>).FullName}";

        FeatureDefinition<,> definition = new(new ServiceCollection());

        Func<FeatureDefinition<,>> act = () => definition.AddExceptionHandlingBehavior<TestPipeline2<Request, Result>>();

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }
}
