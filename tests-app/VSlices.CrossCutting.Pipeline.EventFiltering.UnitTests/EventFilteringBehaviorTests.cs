using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Globalization;
using VSlices.Base.Failures;
using VSlices.Core;
using VSlices.CrossCutting.Pipeline.EventFiltering.MessageTemplates;
using VSlices.Domain;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Pipeline.EventFiltering.UnitTests;

public class EventFilteringBehaviorTests
{
    public sealed record Request : Event;

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

    public class Handler : IHandler<Request>
    {
        public Aff<Runtime, Unit> Define(Request request)
        {
            throw new NotImplementedException();
        }
    }

    readonly Logger _logger = Substitute.For<Logger>();
    readonly IEventFilter<Request, Handler> _eventFilter = Substitute.For<IEventFilter<Request, Handler>>();
    readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    EventFilteringBehavior<Request, Handler> GetSut(IEventFilteringMessageTemplate template) 
        => new(_eventFilter, template, _logger, _timeProvider);

    public static IEnumerable<object[]> GetTemplates()
    {
        return
        [
            [EnglishEventFilteringMessageTemplate.Instance],
            [SpanishEventFilteringMessageTemplate.Instance]
        ];
    }

    [Theory]
    [MemberData(nameof(GetTemplates))]
    public async Task Define_Success_ShouldLogInputAndSuccessOutput(IEventFilteringMessageTemplate template)
    {
        // Arrange
        EventFilteringBehavior<Request, Handler> sut = GetSut(template);

        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;
        Request request = new();

        string expStartMessage = string.Format(template.ContinueExecution,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Handler).FullName, request);

        _eventFilter.Define(request)
            .Returns(trueAff);

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

    }

    [Theory]
    [MemberData(nameof(GetTemplates))]
    public async Task Define_Success_ShouldLogInputAndFailureOutput(IEventFilteringMessageTemplate template)
    {
        // Arrange
        EventFilteringBehavior<Request, Handler> sut = GetSut(template);

        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;
        Request request = new();

        string expStartMessage = string.Format(template.SkipExecution,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Handler).FullName, request);

        _eventFilter.Define(request)
            .Returns(falseAff);

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
    }
}