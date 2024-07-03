using System.Globalization;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VSlices.Base.Failures;
using VSlices.Core.UseCases;
using VSlices.CrossCutting.Pipeline.Logging.MessageTemplates;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Pipeline.Logging.UnitTests;

public class LoggingBehaviorTests
{
    public sealed record Request : IRequest;

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

        public virtual bool IsEnabled(LogLevel logLevel) => true;

        public abstract IDisposable BeginScope<TState>(TState state);
    }

    readonly Logger _logger = Substitute.For<Logger>();

    readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    LoggingBehavior<Request, Unit> GetSut(ILoggingMessageTemplate template) => new(template, _logger, _timeProvider);

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
        LoggingBehavior<Request, Unit> sut = GetSut(template);
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;
        Request request = new();

        string expStartMessage = string.Format(template.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        string expSuccessEndMessage = string.Format(template.SuccessEnd,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request, unit);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        // Act
        Fin<Unit> result = await sut
            .Define(request, SuccessAff(unit))
            .Run(Runtime.New());

        // Assert
        result.IsSucc.Should().BeTrue();

        _logger.Received(1).Log(
            LogLevel.Information, 
            expStartMessage);

        _logger.Received(1).Log(
            LogLevel.Information,
            expSuccessEndMessage);
    }

    [Theory]
    [MemberData(nameof(GetTemplates))]
    public async Task Define_Success_ShouldLogInputAndFailureOutput(ILoggingMessageTemplate template)
    {
        // Arrange
        LoggingBehavior<Request, Unit> sut = GetSut(template);
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
        Fin<Unit> result = await sut
            .Define(request, EffMaybe<Unit>(() => expError))
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
        LoggingBehavior<Request, Unit> sut = GetSut(template);
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
        Fin<Unit> result = await sut
            .Define(request, EffMaybe<Unit>(() => expError))
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