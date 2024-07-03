using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Globalization;
using VSlices.Base.Failures;
using VSlices.Core.Stream;
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

        public abstract IDisposable BeginScope<TState>(TState state);
    }

    private readonly Logger _logger = Substitute.For<Logger>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    LoggingStreamBehavior<Request, Response> GetSut(ILoggingMessageTemplate template) => new(template, _logger, _timeProvider);

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
    public async Task Define_Success_ShouldLogInputAndSuccessOutput(ILoggingMessageTemplate template)
    {
        // Arrange
        LoggingStreamBehavior<Request, Response> sut = GetSut(template);
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;
        Request request = new();

        string expStartMessage = string.Format(template.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        string expSuccessEndMessage = string.Format(template.SuccessEnd,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        // Act
        Fin<IAsyncEnumerable<Response>> result = await sut
            .Define(request, SuccessAff(Yield()))
            .Run(Runtime.New());

        // Assert
        result.IsSucc.Should().BeTrue();

        _logger.Received(1).Log(
            LogLevel.Information,
            expStartMessage);

        _logger.Received(1).Log(
            LogLevel.Information,
            expSuccessEndMessage);

        return;

        async IAsyncEnumerable<Response> Yield()
        {
            yield return new Response();
        }
    }

    [Theory]
    [MemberData(nameof(GetTemplates))]
    public async Task Define_Success_ShouldLogInputAndFailureOutput(ILoggingMessageTemplate template)
    {
        // Arrange
        LoggingStreamBehavior<Request, Response> sut = GetSut(template);
        DateTimeOffset expFirstTime = DateTimeOffset.Now;

        Request request = new();
        Error expError = new NotFound("NotFound");

        string expStartMessage = string.Format(template.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        string expFailureEndMessage = string.Format(template.FailureEnd,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request, expError);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        // Act
        Fin<IAsyncEnumerable<Response>> result = await sut
            .Define(request, EffMaybe<IAsyncEnumerable<Response>>(() => expError))
            .Run(Runtime.New());

        // Assert
        result.IsSucc.Should().BeFalse();

        _logger.Received(1).Log(
            LogLevel.Information,
            expStartMessage);

        _logger.Received(1).Log(
            LogLevel.Warning,
            expFailureEndMessage);
    }

    [Theory]
    [MemberData(nameof(GetTemplates))]
    public async Task Define_Failure_ShouldLogInputAndFailureOutput(ILoggingMessageTemplate template)
    {
        // Arrange
        LoggingStreamBehavior<Request, Response> sut = GetSut(template);
        DateTimeOffset expFirstTime = DateTimeOffset.Now;

        Request request = new();
        Error expError = Error.New(new Exception("Unexpected error occurred"));

        string expStartMessage = string.Format(template.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        string expFailureEndMessage = string.Format(template.FailureEnd,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request, expError);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        // Act
        Fin<IAsyncEnumerable<Response>> result = await sut
            .Define(request, EffMaybe<IAsyncEnumerable<Response>>(() => expError))
            .Run(Runtime.New());

        // Assert
        result.IsSucc.Should().BeFalse();

        _logger.Received(1).Log(
            LogLevel.Information,
            expStartMessage);

        _logger.Received(1).Log(
            LogLevel.Error,
            expFailureEndMessage);
    }
}