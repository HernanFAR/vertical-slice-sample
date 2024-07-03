using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Core.Builder;
using VSlices.CrossCutting.Pipeline.Logging.MessageTemplates;

namespace VSlices.CrossCutting.Pipeline.Logging.UnitTests.Extensions;

public class LoggingBehaviorExtensionsTests
{
    public record Result;
    public record Request : IFeature<Result>;

    public class CustomTemplate : ILoggingMessageTemplate
    {
        public string Start { get; }
        public string FailureEnd { get; }
        public string SuccessEnd { get; }
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldRegisterInServiceContainer_DetailWithSpanishTemplate()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddLoggingBehaviorFor<Request>()
            .UsingSpanishTemplate();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(LoggingBehavior<Request, Result>))
            .Should().BeTrue();

        builder.Services
            .Where(e => e.ServiceType == typeof(ILoggingMessageTemplate))
            .Any(e => e.ImplementationType == typeof(SpanishLoggingMessageTemplate))
            .Should().BeTrue();
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldRegisterInServiceContainer_DetailWithEnglishTemplate()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddLoggingBehaviorFor<Request>()
            .UsingEnglishTemplate();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(LoggingBehavior<Request, Result>))
            .Should().BeTrue();

        builder.Services
            .Where(e => e.ServiceType == typeof(ILoggingMessageTemplate))
            .Any(e => e.ImplementationType == typeof(EnglishLoggingMessageTemplate))
            .Should().BeTrue();
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldRegisterInServiceContainer_DetailWithCustomTemplate()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddLoggingBehaviorFor<Request>()
            .UsingTemplate<CustomTemplate>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(LoggingBehavior<Request, Result>))
            .Should().BeTrue();

        builder.Services
            .Where(e => e.ServiceType == typeof(ILoggingMessageTemplate))
            .Any(e => e.ImplementationType == typeof(CustomTemplate))
            .Should().BeTrue();
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldThrowExceptoin()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        Func<LoggingBehaviorBuilder> act = () => builder.AddLoggingBehaviorFor(typeof(object));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage($"The type {typeof(object).FullName} does not implement {typeof(IFeature<>).FullName}");
    }
}
