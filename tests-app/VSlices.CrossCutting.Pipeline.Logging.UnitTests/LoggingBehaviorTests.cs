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

    LoggingBehavior<Request, Unit> GetSut() => new(
        EnglishLoggingMessageTemplate.Instance,
        _logger,
        _timeProvider
    );

    [Fact]
    public async Task Define_Success_ShouldLogInputAndSuccessOutput()
    {
        // Arrange
        LoggingBehavior<Request, Unit> sut = GetSut();
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;
        Request request = new();

        string expStartMessage = string.Format(EnglishLoggingMessageTemplate.Instance.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        string expSuccessEndMessage = string.Format(EnglishLoggingMessageTemplate.Instance.SuccessEnd,
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

    [Fact]
    public async Task Define_Success_ShouldLogInputAndFailureOutput()
    {
        // Arrange
        LoggingBehavior<Request, Unit> sut = GetSut();
        DateTimeOffset expFirstTime = DateTimeOffset.Now;

        Request request = new();
        Error expError = new NotFound("NotFound");

        string expStartMessage = string.Format(EnglishLoggingMessageTemplate.Instance.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        string expFailureEndMessage = string.Format(EnglishLoggingMessageTemplate.Instance.FailureEnd,
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

    [Fact]
    public async Task Define_Failure_ShouldLogInputAndFailureOutput()
    {
        // Arrange
        LoggingBehavior<Request, Unit> sut = GetSut();
        DateTimeOffset expFirstTime = DateTimeOffset.Now;

        Request request = new();
        Error expError = Error.New(new Exception("Unexpected error occurred"));

        string expStartMessage = string.Format(EnglishLoggingMessageTemplate.Instance.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        string expFailureEndMessage = string.Format(EnglishLoggingMessageTemplate.Instance.FailureEnd,
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