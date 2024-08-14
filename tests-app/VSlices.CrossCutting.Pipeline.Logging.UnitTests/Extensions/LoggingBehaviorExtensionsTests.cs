using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Core.Builder;
using VSlices.CrossCutting.Pipeline.Logging.MessageTemplates;

namespace VSlices.CrossCutting.Pipeline.Logging.UnitTests.Extensions;

public sealed class LoggingBehaviorExtensionsTests
{
    public sealed record Result;
    public sealed record Request : IFeature<Result>;

    public sealed class Handler : IHandler<Request, Result>
    {
        public Eff<VSlicesRuntime, Result> Define(Request input)
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

        var chain = new BehaviorChain(services, typeof(Request), typeof(Result), typeof(Handler));

        // Act
        chain.AddLogging().InSpanish();

        // Assert
        services.Where(e => e.ImplementationType == typeof(LoggingBehavior<,>))
                .Any(e => e.Lifetime             == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType        == typeof(ILoggingMessageTemplate))
                .Where(e => e.ImplementationType == typeof(SpanishLoggingMessageTemplate))
                .Any(e => e.Lifetime             == ServiceLifetime.Singleton)
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(LoggingBehavior<Request, Result>));
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldRegisterInServiceContainer_DetailWithEnglishTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        var chain = new BehaviorChain(services, typeof(Request), typeof(Result), typeof(Handler));

        // Act
        chain.AddLogging().InEnglish();

        // Assert
        services.Where(e => e.ImplementationType == typeof(LoggingBehavior<,>))
                .Any(e => e.Lifetime             == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType        == typeof(ILoggingMessageTemplate))
                .Where(e => e.ImplementationType == typeof(EnglishLoggingMessageTemplate))
                .Any(e => e.Lifetime             == ServiceLifetime.Singleton)
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(LoggingBehavior<Request, Result>));
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldRegisterInServiceContainer_DetailWithCustomTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        var services = new ServiceCollection();

        var chain = new BehaviorChain(services, typeof(Request), typeof(Result), typeof(Handler));
        
        // Act
        chain.AddLogging().In<CustomTemplate>();

        // Assert
        services.Where(e => e.ImplementationType == typeof(LoggingBehavior<,>))
                .Any(e => e.Lifetime             == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType        == typeof(ILoggingMessageTemplate))
                .Where(e => e.ImplementationType == typeof(CustomTemplate))
                .Any(e => e.Lifetime             == ServiceLifetime.Singleton)
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(LoggingBehavior<Request, Result>));
    }
}
