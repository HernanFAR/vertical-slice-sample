using System.Globalization;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VSlices.Base.Failures;
using VSlices.Core;
using VSlices.Core.Traits;
using VSlices.Core.UseCases;
using VSlices.CrossCutting.Pipeline.Logging.MessageTemplates;
using VSlices.CrossCutting.Pipeline.Logging.UnitTests.Extensions;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Pipeline.Logging.UnitTests;

public class LoggingBehaviorTests
{
    public sealed record Request : IRequest;

    public abstract class Logger : ILogger<Request>
    {
        void ILogger.Log<TState1>(
            LogLevel logLevel, 
            EventId eventId, 
            TState1 state, 
            Exception? exception, 
            Func<TState1, Exception?, string> formatter)
        {
            string message = formatter(state, exception);
            Log(logLevel, message);
        }

        public abstract void Log(LogLevel logLevel, string message);

        public virtual bool IsEnabled(LogLevel logLevel) => true;

        public abstract IDisposable? BeginScope<TState1>(TState1 state)
            where TState1 : notnull;
    }

    readonly Logger _logger = Substitute.For<Logger>();

    readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    
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
        LoggingBehavior<Request, Unit> sut = new();
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;
        Request request = new();

        string expStartMessage = string.Format(template.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        string expSuccessEndMessage = string.Format(template.SuccessEnd,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request, unit);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        ServiceProvider provider = new ServiceCollection()
                                   .AddSingleton<ILogger<Request>>(_logger)
                                   .AddSingleton(TimeProvider.System)
                                   .AddSingleton(template)
                                   .BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var                runtime            = HandlerRuntime.New(dependencyProvider);

        // Act
        Fin<Unit> result = sut.Define(request, SuccessEff(unit))
                              .Run(runtime, default(CancellationToken));

        // Assert
        result.IsSucc.Should().BeTrue();

        _logger.Received(1).Log(
            LogLevel.Information, 
            expStartMessage);

        _logger.Received(1).Log(
            LogLevel.Information,
            expSuccessEndMessage);
        return Task.CompletedTask;
    }

    [Theory]
    [MemberData(nameof(GetTemplates))]
    public Task Define_Success_ShouldLogInputAndFailureOutput(ILoggingMessageTemplate template)
    {
        // Arrange
        LoggingBehavior<Request, Unit> sut = new();
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
        var                runtime            = HandlerRuntime.New(dependencyProvider);

        // Act
        Fin<Unit> result = sut.Define(request, liftEff<Unit>(() => expError))
                              .Run(runtime, default(CancellationToken));

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
        LoggingBehavior<Request, Unit> sut = new();
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
        var                runtime            = HandlerRuntime.New(dependencyProvider);

        // Act
        Fin<Unit> result = sut.Define(request, liftEff<Unit>(() => expError))
                              .Run(runtime, default(CancellationToken));

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