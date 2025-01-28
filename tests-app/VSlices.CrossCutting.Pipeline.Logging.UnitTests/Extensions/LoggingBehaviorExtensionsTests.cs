using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Core;
using VSlices.Base.Definitions;
using VSlices.Core.Builder;
using VSlices.CrossCutting.Interceptor.Logging.MessageTemplates;

namespace VSlices.CrossCutting.Interceptor.Logging.UnitTests.Extensions;

public sealed class LoggingBehaviorExtensionsTests
{
    public sealed record Result;
    public sealed record Input;

    public sealed class Behavior : IBehavior<Input, Result>
    {
        public Eff<VSlicesRuntime, Result> Define(Input input)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class CustomTemplate : ILoggingMessageTemplate
    {
        public string Start { get; } = string.Empty;
        public string FailureEnd { get; } = string.Empty;
        public string SuccessEnd { get; } = string.Empty;
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldRegisterInServiceContainer_DetailWithSpanishTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        InterceptorChain<Input, Result, Behavior> chain = new(services);

        // Act
        chain.AddLogging().InSpanish();

        // Assert
        services.Where(e => e.ImplementationType == typeof(LoggingInterceptor<,>))
                .Any(e => e.Lifetime             == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType        == typeof(ILoggingMessageTemplate))
                .Where(e => e.ImplementationType == typeof(SpanishLoggingMessageTemplate))
                .Any(e => e.Lifetime             == ServiceLifetime.Singleton)
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(LoggingInterceptor<Input, Result>));
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldRegisterInServiceContainer_DetailWithEnglishTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        var chain = new InterceptorChain<Input, Result, Behavior>(services);

        // Act
        chain.AddLogging().InEnglish();

        // Assert
        services.Where(e => e.ImplementationType == typeof(LoggingInterceptor<,>))
                .Any(e => e.Lifetime             == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType        == typeof(ILoggingMessageTemplate))
                .Where(e => e.ImplementationType == typeof(EnglishLoggingMessageTemplate))
                .Any(e => e.Lifetime             == ServiceLifetime.Singleton)
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(LoggingInterceptor<Input, Result>));
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldRegisterInServiceContainer_DetailWithCustomTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        var chain = new InterceptorChain<Input, Result, Behavior>(services);
        
        // Act
        chain.AddLogging().In<CustomTemplate>();

        // Assert
        services.Where(e => e.ImplementationType == typeof(LoggingInterceptor<,>))
                .Any(e => e.Lifetime             == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType        == typeof(ILoggingMessageTemplate))
                .Where(e => e.ImplementationType == typeof(CustomTemplate))
                .Any(e => e.Lifetime             == ServiceLifetime.Singleton)
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(LoggingInterceptor<Input, Result>));
    }
}
