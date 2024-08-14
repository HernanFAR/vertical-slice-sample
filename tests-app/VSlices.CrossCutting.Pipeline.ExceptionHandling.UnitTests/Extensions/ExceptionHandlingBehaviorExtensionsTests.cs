using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Core;
using VSlices.Core.Builder;
using VSlices.CrossCutting.Pipeline.ExceptionHandling.MessageTemplates;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling.UnitTests.Extensions;

public class ExceptionHandlingBehaviorExtensionsTests
{
    public record FalsePipeline : IPipelineBehavior;

    public record Result;

    public record Request : IFeature<Result>;
    
    public record Handler : IHandler<Request, Result>
    {
        public Eff<VSlicesRuntime, Result> Define(Request input)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class CustomTemplate : IExceptionMessageTemplate
    {
        public string LogException => throw new NotImplementedException();

        public string ErrorMessage => throw new NotImplementedException();
    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldRegisterInServiceContainer_DetailWithEnglishTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        var chain = new BehaviorChain(services, typeof(Request), typeof(Result), typeof(Handler));

        // Act
        chain.AddLoggingException().InEnglish();

        // Arrange
        services.Where(e => e.ServiceType      == typeof(LoggingExceptionBehavior<,>))
                .Any(e => e.Lifetime == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType == typeof(TimeProvider))
                .Any(e => e.Lifetime      == ServiceLifetime.Singleton)
                .Should().BeTrue();

        services.Where(e => e.ServiceType        == typeof(IExceptionMessageTemplate))
                .Where(e => e.ImplementationType == typeof(EnglishExceptionMessageTemplate))
                .Any(e => e.Lifetime             == ServiceLifetime.Singleton)
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(LoggingExceptionBehavior<Request, Result>));

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldRegisterInServiceContainer_DetailWithSpanishTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        var chain = new BehaviorChain(services, typeof(Request), typeof(Result), typeof(Handler));

        // Act
        chain.AddLoggingException().InSpanish();

        // Arrange
        services.Where(e => e.ServiceType == typeof(LoggingExceptionBehavior<,>))
                .Any(e => e.Lifetime      == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType == typeof(TimeProvider))
                .Any(e => e.Lifetime      == ServiceLifetime.Singleton)
                .Should().BeTrue();

        services.Where(e => e.ServiceType        == typeof(IExceptionMessageTemplate))
                .Where(e => e.ImplementationType == typeof(SpanishExceptionMessageTemplate))
                .Any(e => e.Lifetime             == ServiceLifetime.Singleton)
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(LoggingExceptionBehavior<Request, Result>));

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldRegisterInServiceContainer_DetailWithCustomTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        var chain = new BehaviorChain(services, typeof(Request), typeof(Result), typeof(Handler));

        // Act
        chain.AddLoggingException().In<CustomTemplate>();

        // Arrange
        services.Where(e => e.ServiceType == typeof(LoggingExceptionBehavior<,>))
                .Any(e => e.Lifetime      == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType == typeof(TimeProvider))
                .Any(e => e.Lifetime      == ServiceLifetime.Singleton)
                .Should().BeTrue();

        services.Where(e => e.ServiceType        == typeof(IExceptionMessageTemplate))
                .Where(e => e.ImplementationType == typeof(CustomTemplate))
                .Any(e => e.Lifetime             == ServiceLifetime.Singleton)
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(LoggingExceptionBehavior<Request, Result>));

    }
}
