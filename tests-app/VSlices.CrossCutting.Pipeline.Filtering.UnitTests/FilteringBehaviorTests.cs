using System.Globalization;
using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VSlices.Base;
using VSlices.Base.Core;
using VSlices.Base.Traits;
using VSlices.CrossCutting.Interceptor.Filtering;
using VSlices.CrossCutting.Interceptor.Filtering.MessageTemplates;
using VSlices.Domain;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Integrator.Filtering.UnitTests;

public sealed class FilteringBehaviorInterceptorTests
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

        public virtual bool IsEnabled(LogLevel logLevel) => true;

        public abstract IDisposable BeginScope<TState1>(TState1 state)
            where TState1 : notnull;
    }

    public delegate bool PassGet();

    public sealed class Filter(PassGet canPass) : IEventFilter<Request, Behavior>
    {
        public Eff<VSlicesRuntime, bool> DefineFilter(Request feature) => liftEff(_ => canPass());
    }

    public sealed class Behavior : IBehavior<Request>
    {
        public Eff<VSlicesRuntime, Unit> Define(Request input) => throw new NotImplementedException();
    }

    private readonly Logger _logger = Substitute.For<Logger>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    public static IEnumerable<object[]> GetTemplates()
    {
        yield return [EnglishEventFilteringMessageTemplate.Instance];
        yield return [SpanishEventFilteringMessageTemplate.Instance];
    }

    [Theory]
    [MemberData(nameof(GetTemplates))]
    public void Define_Success_ShouldLogInputAndSuccessOutput(IEventFilteringMessageTemplate template)
    {
        // Arrange
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;
        FilteringBehaviorInterceptor<Request, Filter, Behavior> sut = new();
        Request request = new();

        IServiceCollection services = new ServiceCollection()
                                      .AddTransient<Filter>()
                                      .AddTransient<PassGet>(_ => () => true)
                                      .AddSingleton(template)
                                      .AddSingleton<ILogger<Request>>(_logger)
                                      .AddSingleton(_timeProvider);

        var dependencyProvider = new DependencyProvider(services.BuildServiceProvider());

        string expStartMessage = string.Format(template.ContinueExecution,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        // Act
        Fin<Unit> result = sut.Define(request, SuccessEff(unit))
                              .Run(VSlicesRuntime.New(dependencyProvider), EnvIO.New());

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
        DateTimeOffset expFirstTime = DateTimeOffset.Now.UtcDateTime;
        FilteringBehaviorInterceptor<Request, Filter, Behavior> sut = new();
        Request request = new();

        IServiceCollection services = new ServiceCollection()
                                      .AddTransient<Filter>()
                                      .AddTransient<PassGet>(_ => () => false)
                                      .AddSingleton(template)
                                      .AddSingleton<ILogger<Request>>(_logger)
                                      .AddSingleton(_timeProvider);

        var dependencyProvider = new DependencyProvider(services.BuildServiceProvider());

        string expStartMessage = string.Format(template.SkipExecution,
            expFirstTime.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture), typeof(Request).FullName, request);

        _timeProvider.GetUtcNow()
            .Returns(expFirstTime);

        // Act
        Fin<Unit> result = sut.Define(request, SuccessEff(unit))
                              .Run(VSlicesRuntime.New(dependencyProvider), EnvIO.New());

        // Assert
        result.IsSucc.Should().BeTrue();

        _logger.Received(1).Log(
            LogLevel.Warning,
            expStartMessage);
    }
}