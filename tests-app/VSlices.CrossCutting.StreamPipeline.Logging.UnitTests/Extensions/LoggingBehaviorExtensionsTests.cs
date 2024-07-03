using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Core.Builder;
using VSlices.Core.Stream;
using VSlices.CrossCutting.StreamPipeline.Logging.MessageTemplates;

namespace VSlices.CrossCutting.StreamPipeline.Logging.UnitTests.Extensions;

public class LoggingBehaviorExtensionsTests
{
    public record Result;
    public record Request : IStream<Result>;

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

        builder.AddLoggingStreamBehaviorFor<Request>()
            .UsingSpanishTemplate();

        builder.Services
            .Where(e => e.ServiceType == typeof(IStreamPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(LoggingStreamBehavior<Request, Result>))
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

        builder.AddLoggingStreamBehaviorFor<Request>()
            .UsingEnglishTemplate();

        builder.Services
            .Where(e => e.ServiceType == typeof(IStreamPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(LoggingStreamBehavior<Request, Result>))
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

        builder.AddLoggingStreamBehaviorFor<Request>()
            .UsingTemplate<CustomTemplate>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IStreamPipelineBehavior<Request, Result>))
            .Any(e => e.ImplementationType == typeof(LoggingStreamBehavior<Request, Result>))
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

        Func<LoggingBehaviorBuilder> act = () => builder.AddLoggingStreamBehaviorFor(typeof(object));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage($"The type {typeof(object).FullName} does not implement {typeof(IStream<>).FullName}");
    }
}
