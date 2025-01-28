using System.Globalization;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Base.Traits;
using VSlices.Core.UseCases;
using VSlices.CrossCutting.Interceptor.Logging.MessageTemplates;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Interceptor.Logging.UnitTests;

public class LoggingInterceptorTests
{
    public sealed record Input : IInput;

    public abstract class Logger : ILogger<Input>
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
        LoggingInterceptor<Input, Unit> sut = new();
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;
        Input input = new();

        string expStartMessage = string.Format(template.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Input).FullName, input);

        string expSuccessEndMessage = string.Format(template.SuccessEnd,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Input).FullName, input, unit);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        ServiceProvider provider = new ServiceCollection()
                                   .AddSingleton<ILogger<Input>>(_logger)
                                   .AddSingleton(_timeProvider)
                                   .AddSingleton(template)
                                   .BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var                runtime            = VSlicesRuntime.New(dependencyProvider);

        // Act
        Fin<Unit> result = sut.Define(input, SuccessEff(unit))
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
        LoggingInterceptor<Input, Unit> sut = new();
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;

        Input input = new();
        ExtensibleExpected exp = ExtensibleExpected.NotFound("NotFound", []);

        string expStartMessage = string.Format(template.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Input).FullName, input);

        string expFailureEndMessage = string.Format(template.FailureEnd,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Input).FullName, input, exp);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        ServiceProvider provider = new ServiceCollection()
                                   .AddVSlicesRuntime()
                                   .AddSingleton<ILogger<Input>>(_logger)
                                   .AddSingleton(_timeProvider)
                                   .AddSingleton(template)
                                   .BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var                runtime            = VSlicesRuntime.New(dependencyProvider);

        // Act
        Fin<Unit> result = sut.Define(input, liftEff<Unit>(() => exp))
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
        LoggingInterceptor<Input, Unit> sut = new();
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;

        Input input = new();
        Error expError = Error.New(new Exception("Unexpected error occurred"));

        string expStartMessage = string.Format(template.Start,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Input).FullName, input);

        string expFailureEndMessage = string.Format(template.FailureEnd,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Input).FullName, input, expError);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        ServiceProvider provider = new ServiceCollection()
                                   .AddSingleton<ILogger<Input>>(_logger)
                                   .AddSingleton(_timeProvider)
                                   .AddSingleton(template)
                                   .BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var                runtime            = VSlicesRuntime.New(dependencyProvider);

        // Act
        Fin<Unit> result = sut.Define(input, liftEff<Unit>(() => expError))
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