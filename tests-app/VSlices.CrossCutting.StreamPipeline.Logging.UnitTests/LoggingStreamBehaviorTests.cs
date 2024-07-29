using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Failures;
using VSlices.Core;
using VSlices.Core.Stream;
using VSlices.Core.Traits;
using VSlices.CrossCutting.StreamPipeline.Logging.MessageTemplates;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.StreamPipeline.Logging.UnitTests;

public class LoggingStreamBehaviorTests
{
    public sealed record Response;
    public sealed record Request : IStream<Response>;

    public abstract class Logger : ILogger<Request>
    {
        void ILogger.Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            string message = formatter(state, exception);
            Log(logLevel, message);
        }

        public abstract void Log(LogLevel logLevel, string message);

        public virtual bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public abstract IDisposable? BeginScope<TState1>(TState1 state)
            where TState1 : notnull;
    }

    private readonly Logger _logger = Substitute.For<Logger>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    public static IEnumerable<object[]> GetTemplates()
    {
        return
        [
            [EnglishLoggingMessageTemplate.Instance],
            [SpanishLoggingMessageTemplate.Instance]
        ];
    }

    [Theory]
    [MemberData(nameof(GetTemplates))]
    public Task Define_Success_ShouldLogInputAndSuccessOutput(ILoggingMessageTemplate template)
    {
        // Arrange
        LoggingStreamBehavior<Request, Response> sut = new();
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;
        Request request = new();

        string expStartMessage = string.Format(template.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        string expSuccessEndMessage = string.Format(template.SuccessEnd,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        ServiceProvider provider = new ServiceCollection()
                                   .AddSingleton<ILogger<Request>>(_logger)
                                   .AddSingleton(TimeProvider.System)
                                   .AddSingleton(template)
                                   .BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var                runtime            = HandlerRuntime.New(dependencyProvider, EnvIO.New());

        // Act
        Fin<IAsyncEnumerable<Response>> result = sut.Define(request, SuccessEff(Yield()))
                                                    .Run(runtime, runtime.EnvIO);

        // Assert
        result.IsSucc.Should().BeTrue();

        _logger.Received(1).Log(
            LogLevel.Information,
            expStartMessage);

        _logger.Received(1).Log(
            LogLevel.Information,
            expSuccessEndMessage);

        return Task.CompletedTask;

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async IAsyncEnumerable<Response> Yield()
            #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            yield return new Response();
        }
    }

    [Theory]
    [MemberData(nameof(GetTemplates))]
    public Task Define_Success_ShouldLogInputAndFailureOutput(ILoggingMessageTemplate template)
    {
        // Arrange
        LoggingStreamBehavior<Request, Response> sut = new();
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;

        Request request = new();
        Error expError = new NotFound("NotFound");

        string expStartMessage = string.Format(template.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        string expFailureEndMessage = string.Format(template.FailureEnd,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request, expError);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        ServiceProvider provider = new ServiceCollection()
                                   .AddSingleton<ILogger<Request>>(_logger)
                                   .AddSingleton(TimeProvider.System)
                                   .AddSingleton(template)
                                   .BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var                runtime            = HandlerRuntime.New(dependencyProvider, EnvIO.New());

        // Act
        Fin<IAsyncEnumerable<Response>> result = sut
            .Define(request, liftEff<IAsyncEnumerable<Response>>(() => expError))
            .Run(runtime, runtime.EnvIO);

        // Assert
        result.IsSucc.Should().BeFalse();

        _logger.Received(1).Log(
            LogLevel.Information,
            expStartMessage);

        _logger.Received(1).Log(
            LogLevel.Warning,
            expFailureEndMessage);
        return Task.CompletedTask;
    }

    [Theory]
    [MemberData(nameof(GetTemplates))]
    public Task Define_Failure_ShouldLogInputAndFailureOutput(ILoggingMessageTemplate template)
    {
        // Arrange
        LoggingStreamBehavior<Request, Response> sut = new();
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;

        Request request = new();
        Error expError = Error.New(new Exception("Unexpected error occurred"));

        string expStartMessage = string.Format(template.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        string expFailureEndMessage = string.Format(template.FailureEnd,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request, expError);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        ServiceProvider provider = new ServiceCollection()
                                   .AddSingleton<ILogger<Request>>(_logger)
                                   .AddSingleton(TimeProvider.System)
                                   .AddSingleton(template)
                                   .BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var                runtime            = HandlerRuntime.New(dependencyProvider, EnvIO.New());

        // Act
        Fin<IAsyncEnumerable<Response>> result = sut.Define(request, liftEff<IAsyncEnumerable<Response>>(() => expError))
                                                    .Run(runtime, runtime.EnvIO);

        // Assert
        result.IsSucc.Should().BeFalse();

        _logger.Received(1).Log(
            LogLevel.Information,
            expStartMessage);

        _logger.Received(1).Log(
            LogLevel.Error,
            expFailureEndMessage);
        return Task.CompletedTask;
    }
}