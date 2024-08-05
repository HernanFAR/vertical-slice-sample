using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Core.Builder;
using VSlices.CrossCutting.Pipeline.Logging.MessageTemplates;

namespace VSlices.CrossCutting.Pipeline.Logging.UnitTests.Extensions;

public class LoggingBehaviorExtensionsTests
{
    public record Result;
    public record Request : IFeature<Result>;

    public class CustomTemplate : ILoggingMessageTemplate
    {
        public string Start { get; } = string.Empty;
        public string FailureEnd { get; } = string.Empty;
        public string SuccessEnd { get; } = string.Empty;
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldRegisterInServiceContainer_DetailWithSpanishTemplate()
    {
        FeatureDefinition<,> definition = new(new ServiceCollection());

        definition.AddLoggingBehaviorFor<Request>()
            .UsingSpanishTemplate();

        definition.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(LoggingBehavior<Request, Result>))
            .Should().BeTrue();

        definition.Services
            .Where(e => e.ServiceType == typeof(ILoggingMessageTemplate))
            .Any(e => e.ImplementationType == typeof(SpanishLoggingMessageTemplate))
            .Should().BeTrue();
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldRegisterInServiceContainer_DetailWithEnglishTemplate()
    {
        FeatureDefinition<,> definition = new(new ServiceCollection());

        definition.AddLoggingBehaviorFor<Request>()
            .UsingEnglishTemplate();

        definition.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(LoggingBehavior<Request, Result>))
            .Should().BeTrue();

        definition.Services
            .Where(e => e.ServiceType == typeof(ILoggingMessageTemplate))
            .Any(e => e.ImplementationType == typeof(EnglishLoggingMessageTemplate))
            .Should().BeTrue();
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldRegisterInServiceContainer_DetailWithCustomTemplate()
    {
        FeatureDefinition<,> definition = new(new ServiceCollection());

        definition.AddLoggingBehaviorFor<Request>()
            .UsingTemplate<CustomTemplate>();

        definition.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(LoggingBehavior<Request, Result>))
            .Should().BeTrue();

        definition.Services
            .Where(e => e.ServiceType == typeof(ILoggingMessageTemplate))
            .Any(e => e.ImplementationType == typeof(CustomTemplate))
            .Should().BeTrue();
    }

    [Fact]
    public void AddLoggingBehaviorFor_ShouldThrowExceptoin()
    {
        FeatureDefinition<,> definition = new(new ServiceCollection());

        Func<LoggingBehaviorBuilder> act = () => definition.AddLoggingBehaviorFor(typeof(object));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage($"The type {typeof(object).FullName} does not implement {typeof(IFeature<>).FullName}");
    }
}
