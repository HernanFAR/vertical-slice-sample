using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Base.Definitions;
using VSlices.Core.Builder;
using VSlices.CrossCutting.Interceptor.ExceptionHandling.MessageTemplates;

namespace VSlices.CrossCutting.Interceptor.ExceptionHandling.UnitTests.Extensions;

public class ExceptionHandlingInterceptorExtensionsTests
{
    public record False : IBehaviorInterceptor;

    public record Result;

    public record Input;
    
    public record Behavior : IBehavior<Input, Result>
    {
        public Eff<VSlicesRuntime, Result> Define(Input input)
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

        InterceptorChain<Input, Result, Behavior> chain = new(services);

        // Act
        chain.AddExceptionHandling().UsingLogging().InEnglish();

        // Arrange
        services.Where(e => e.ServiceType      == typeof(LoggingExceptionInterceptor<,>))
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
             .And.Contain(type => type == typeof(LoggingExceptionInterceptor<Input, Result>));

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldRegisterInServiceContainer_DetailWithSpanishTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        InterceptorChain<Input, Result, Behavior> chain = new(services);

        // Act
        chain.AddExceptionHandling().UsingLogging().InSpanish();

        // Arrange
        services.Where(e => e.ServiceType == typeof(LoggingExceptionInterceptor<,>))
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
             .And.Contain(type => type == typeof(LoggingExceptionInterceptor<Input, Result>));

    }

    [Fact]
    public void AddExceptionHandlingPipeline_ShouldRegisterInServiceContainer_DetailWithCustomTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        InterceptorChain<Input, Result, Behavior> chain = new(services);

        // Act
        chain.AddExceptionHandling().UsingLogging().In<CustomTemplate>();

        // Arrange
        services.Where(e => e.ServiceType == typeof(LoggingExceptionInterceptor<,>))
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
             .And.Contain(type => type == typeof(LoggingExceptionInterceptor<Input, Result>));

    }
}
