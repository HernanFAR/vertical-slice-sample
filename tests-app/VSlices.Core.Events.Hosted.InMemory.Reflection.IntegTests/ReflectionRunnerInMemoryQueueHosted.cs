using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Domain;
using static LanguageExt.Prelude;
using static VSlices.VSlicesPrelude;

namespace VSlices.Core.Events.IntegTests;

public class ReflectionRunnerInMemoryQueueHosted
{
    public sealed class Accumulator
    {
        public int Value { get; set; }
    }

    public sealed record AlwaysUnitEvent : Event;

    public sealed class AlwaysUnitRequestHandler : IHandler<AlwaysUnitEvent>
    {
        public Eff<VSlicesRuntime, Unit> Define(AlwaysUnitEvent input) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() => accumulator.Value++)
            select unit;
    }

    public sealed record FirstFailureThenUnitEvent : Event;

    public sealed class FirstFailureThenUnitRequestHandler : IHandler<FirstFailureThenUnitEvent>
    {
        public Eff<VSlicesRuntime, Unit> Define(FirstFailureThenUnitEvent input) =>
            from accumulator in provide<Accumulator>()
            from _1 in guardnot(accumulator.Value == 0, () =>
            {
                accumulator.Value++;
                throw new Exception("First failure");
            })
            from _2 in liftEff(() => accumulator.Value++)
            select unit;
        
    }

    public sealed record FirstAndSecondFailureThenUnitEvent : Event;

    public sealed class FirstAndSecondFailureThenUnitRequestHandler : IHandler<FirstAndSecondFailureThenUnitEvent>
    {
        public Eff<VSlicesRuntime, Unit> Define(FirstAndSecondFailureThenUnitEvent input) =>
            from accumulator in provide<Accumulator>()
            from _1 in guardnot(accumulator.Value == 0, () =>
            {
                accumulator.Value++;
                throw new Exception("First failure");
            })
            from _2 in guardnot(accumulator.Value == 1, () =>
            {
                accumulator.Value++;
                throw new Exception("Second failure");
            })
            from _3 in liftEff(() => accumulator.Value++)
            select unit;
    }

    public sealed record AlwaysFailureEvent : Event;

    public sealed class AlwaysFailureRequestHandler : IHandler<AlwaysFailureEvent>
    {
        public Eff<VSlicesRuntime, Unit> Define(AlwaysFailureEvent input) => throw new Exception("Always failure");
    }

    [Fact]
    public async Task InMemoryEventFlow_AddedEventBeforeListeningStart_AlwaysUnitHandler()
    {
        // Arrange
        const int expCount = 1;
        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddEventListener().WithNoActionInDeadLetterCase()
                       .AddHostedTaskListener()
                       .AddInMemoryEventQueue()
                       .AddReflectionEventRunner()
                       .AddLogging()
                       .AddSingleton<Accumulator>();

        new FeatureDefinition<AlwaysUnitEvent, Unit>(services)
            .Execute<AlwaysUnitRequestHandler>();

        var provider  = services.BuildServiceProvider();

        var backgroundEventListener = provider.GetRequiredService<IHostedService>();
        var eventQueue = provider.GetRequiredService<IEventQueueWriter>();
        var accumulator = provider.GetRequiredService<Accumulator>();

        var event1 = new AlwaysUnitEvent();

        await eventQueue.EnqueueAsync(event1);

        // Act
        _ = backgroundEventListener.StartAsync(default);

        // Assert
        accumulator.Value.Should().Be(expCount);
    }

    [Fact]
    public async Task InMemoryEventFlow_AddedEventAfterListeningStart()
    {
        // Arrange
        const int expCount = 1;
        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddEventListener().WithNoActionInDeadLetterCase()
                       .AddHostedTaskListener()
                       .AddInMemoryEventQueue()
                       .AddReflectionEventRunner()
                       .AddLogging()
                       .AddSingleton<Accumulator>();

        new FeatureDefinition<AlwaysUnitEvent, Unit>(services)
            .Execute<AlwaysUnitRequestHandler>();

        var provider = services.BuildServiceProvider();

        var backgroundEventListener = provider.GetRequiredService<IHostedService>();
        var eventQueue = provider.GetRequiredService<IEventQueueWriter>();
        var accumulator = provider.GetRequiredService<Accumulator>();

        var event2 = new AlwaysUnitEvent();

        _ = backgroundEventListener.StartAsync(default);

        // Act
        await eventQueue.EnqueueAsync(event2);
        await Task.Delay(1000);

        // Assert
        accumulator.Value.Should().Be(expCount);

    }

    [Fact]
    public async Task InMemoryEventFlow_FirstRetry()
    {
        // Arrange
        const int expCount = 2;
        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddEventListener().WithNoActionInDeadLetterCase()
                       .AddHostedTaskListener()
                       .AddInMemoryEventQueue()
                       .AddReflectionEventRunner()
                       .AddLogging()
                       .AddSingleton<Accumulator>();

        new FeatureDefinition<FirstFailureThenUnitEvent, Unit>(services)
            .Execute<FirstFailureThenUnitRequestHandler>();

        var provider = services.BuildServiceProvider();

        var backgroundEventListener = provider.GetRequiredService<IHostedService>();
        var eventQueue = provider.GetRequiredService<IEventQueueWriter>();
        var accumulator = provider.GetRequiredService<Accumulator>();

        var event2 = new FirstFailureThenUnitEvent();

        _ = backgroundEventListener.StartAsync(default);

        // Act
        await eventQueue.EnqueueAsync(event2);
        await Task.Delay(1000);

        // Assert
        accumulator.Value.Should().Be(expCount);
    }

    [Fact]
    public async Task InMemoryEventFlow_SecondRetry()
    {
        // Arrange
        const int expCount = 3;
        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddEventListener().WithNoActionInDeadLetterCase()
                       .AddHostedTaskListener()
                       .AddInMemoryEventQueue()
                       .AddReflectionEventRunner()
                       .AddLogging()
                       .AddSingleton<Accumulator>();

        new FeatureDefinition<FirstAndSecondFailureThenUnitEvent, Unit>(services)
            .Execute<FirstAndSecondFailureThenUnitRequestHandler>();

        var provider = services.BuildServiceProvider();
        
        var backgroundEventListener = provider.GetRequiredService<IHostedService>();
        var eventQueue = provider.GetRequiredService<IEventQueueWriter>();
        var accumulator = provider.GetRequiredService<Accumulator>();

        var event2 = new FirstAndSecondFailureThenUnitEvent();

        _ = backgroundEventListener.StartAsync(default);

        // Act
        await eventQueue.EnqueueAsync(event2);
        await Task.Delay(1000);

        // Assert
        accumulator.Value.Should().Be(expCount);

    }

    [Fact]
    public async Task InMemoryEventFlow_NeverMakesIt()
    {
        // Arrange
        var logger = Mock.Of<ILogger<EventListenerBackgroundTask>>();
        Mock<ILogger<EventListenerBackgroundTask>>? loggerMock = Mock.Get(logger);

        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddEventListener().WithNoActionInDeadLetterCase()
                       .AddHostedTaskListener()
                       .AddInMemoryEventQueue()
                       .AddReflectionEventRunner()
                       .AddScoped(_ => logger);

        new FeatureDefinition<AlwaysFailureEvent, Unit>(services)
            .Execute<AlwaysFailureRequestHandler>();

        var provider = services.BuildServiceProvider();

        var backgroundEventListener = provider.GetRequiredService<IHostedService>();
        var eventQueue = provider.GetRequiredService<IEventQueueWriter>();

        var event2 = new AlwaysFailureEvent();

        _ = backgroundEventListener.StartAsync(default);

        // Act
        await eventQueue.EnqueueAsync(event2);

        await Task.Delay(6000);

        // Assert
        loggerMock.Verify(
                          e => e.Log(
                                     LogLevel.Error,
                                     It.IsAny<EventId>(),
                                     It.Is<It.IsAnyType>((o, t) =>
                                                             o.ToString()!.IndexOf($"Max retries 3 reached for {event2}.", StringComparison.Ordinal) != -1),
                                     It.IsAny<Exception>(),
                                     It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                                    ),
                          Times.Once);

    }
}