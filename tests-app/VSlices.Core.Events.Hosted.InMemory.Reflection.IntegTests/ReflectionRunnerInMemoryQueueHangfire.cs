using FluentAssertions;
using Hangfire;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VSlices.Domain;
using static LanguageExt.Prelude;

namespace VSlices.Core.Events.IntegTests;

public class ReflectionRunnerInMemoryQueueHangfire
{
    public record AlwaysUnitEvent : Event;

    public class AlwaysUnitHandler : IHandler<AlwaysUnitEvent>
    {
        public AutoResetEvent HandledEvent { get; } = new(false);

        [Obsolete]
        public Aff<Runtime, Unit> Define(AlwaysUnitEvent request)
        {
            return from _ in Eff(() =>
            {
                HandledEvent.Set();

                return unit;
            })
                   select unit;
        }

        Eff<HandlerRuntime, Unit> IHandler<AlwaysUnitEvent, Unit>.Define(AlwaysUnitEvent request)
        {
            throw new NotImplementedException();
        }
    }

    public record FirstFailureThenUnitEvent : Event;

    public class FirstFailureThenUnitHandler : IHandler<FirstFailureThenUnitEvent>
    {
        public AutoResetEvent HandledEvent { get; } = new(false);

        public bool First { get; set; } = true;

        [Obsolete]
        public Aff<Runtime, Unit> Define(FirstFailureThenUnitEvent request)
        {
            return from _1 in guardnot(First, () =>
            {
                First = false;
                throw new Exception("First failure");
            })
                   from _2 in Eff(() =>
                   {
                       HandledEvent.Set();

                       return unit;
                   })
                   select unit;
        }

        Eff<HandlerRuntime, Unit> IHandler<FirstFailureThenUnitEvent, Unit>.Define(FirstFailureThenUnitEvent request)
        {
            throw new NotImplementedException();
        }
    }

    public record FirstAndSecondFailureThenUnitEvent : Event;

    public class FirstAndSecondFailureThenUnitHandler : IHandler<FirstAndSecondFailureThenUnitEvent>
    {
        public AutoResetEvent HandledEvent { get; } = new(false);

        public bool First { get; set; } = true;

        public bool Second { get; set; } = true;

        [Obsolete]
        public Aff<Runtime, Unit> Define(FirstAndSecondFailureThenUnitEvent request)
        {
            return from _1 in Eff(() =>
            {
                if (!First) return unit;

                First = false;
                throw new Exception("First failure");
            })
                   from _2 in Eff(() =>
                   {
                       if (!Second) return unit;

                       Second = false;
                       throw new Exception("Second failure");
                   })
                   from _3 in Eff(() =>
                   {
                       HandledEvent.Set();

                       return unit;
                   })
                   select unit;
        }
    }

    public record AlwaysFailureEvent : Event;

    public class AlwaysFailureHandler : IHandler<AlwaysFailureEvent>
    {
        public Aff<Runtime, Unit> Define(AlwaysFailureEvent request)
        {
            throw new Exception("Always failure");
        }
    }

    [Fact]
    public async Task InMemoryEventFlow_AddedEventBeforeListeningStart_AlwaysUnitHandler()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddEventListener()
            .AddHangfireTaskListener(config => config.UseInMemoryStorage())
            .AddInMemoryEventQueue()
            .AddReflectionEventRunner()
            .AddLogging()
            .AddSingleton<AlwaysUnitHandler>()
            .AddScoped<IHandler<AlwaysUnitEvent, Unit>>(s => s.GetRequiredService<AlwaysUnitHandler>())
            .BuildServiceProvider();

        IEnumerable<IHostedService> backgroundEventListener = provider.GetServices<IHostedService>();
        InMemoryEventQueue eventQueue = (InMemoryEventQueue)provider.GetRequiredService<IEventQueueWriter>();

        AlwaysUnitEvent event1 = new();

        foreach (IHostedService service in backgroundEventListener)
        {
            _ = service.StartAsync(default);
        }

        await eventQueue.EnqueueAsync(event1, default);

        AlwaysUnitHandler handler = provider.GetRequiredService<AlwaysUnitHandler>();
        handler.HandledEvent.WaitOne(5000).Should().BeTrue();
    }

    [Fact]
    public async Task InMemoryEventFlow_AddedEventAfterListeningStart()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddEventListener()
            .AddHangfireTaskListener(config => config.UseInMemoryStorage())
            .AddInMemoryEventQueue()
            .AddReflectionEventRunner()
            .AddLogging()
            .AddSingleton<AlwaysUnitHandler>()
            .AddScoped<IHandler<AlwaysUnitEvent, Unit>>(s => s.GetRequiredService<AlwaysUnitHandler>())
            .BuildServiceProvider();

        IEnumerable<IHostedService> backgroundEventListener = provider.GetServices<IHostedService>();
        InMemoryEventQueue eventQueue = (InMemoryEventQueue)provider.GetRequiredService<IEventQueueWriter>();

        AlwaysUnitEvent event2 = new();

        foreach (IHostedService service in backgroundEventListener)
        {
            _ = service.StartAsync(default);
        }

        await Task.Delay(1000);

        await eventQueue.EnqueueAsync(event2, default);

        AlwaysUnitHandler handler = provider.GetRequiredService<AlwaysUnitHandler>();
        handler.HandledEvent.WaitOne(5000).Should().BeTrue();
    }

    [Fact]
    public async Task InMemoryEventFlow_FirstRetry()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddEventListener()
            .AddHangfireTaskListener(config => config.UseInMemoryStorage())
            .AddInMemoryEventQueue()
            .AddReflectionEventRunner()
            .AddLogging()
            .AddSingleton<FirstFailureThenUnitHandler>()
            .AddScoped<IHandler<FirstFailureThenUnitEvent, Unit>>(s => s.GetRequiredService<FirstFailureThenUnitHandler>())
            .BuildServiceProvider();

        IEnumerable<IHostedService> services = provider.GetServices<IHostedService>();
        InMemoryEventQueue eventQueue = (InMemoryEventQueue)provider.GetRequiredService<IEventQueueWriter>();

        FirstFailureThenUnitEvent event2 = new();

        foreach (IHostedService hostedService in services)
        {
            _ = hostedService.StartAsync(default);
        }

        await Task.Delay(1000);

        await eventQueue.EnqueueAsync(event2, default);

        FirstFailureThenUnitHandler handler = provider.GetRequiredService<FirstFailureThenUnitHandler>();

        handler.HandledEvent.WaitOne(5000).Should().BeTrue();
    }

    [Fact]
    public async Task InMemoryEventFlow_SecondRetry()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddEventListener()
            .AddHangfireTaskListener(config => config.UseInMemoryStorage())
            .AddInMemoryEventQueue()
            .AddReflectionEventRunner()
            .AddLogging()
            .AddSingleton<FirstAndSecondFailureThenUnitHandler>()
            .AddScoped<IHandler<FirstAndSecondFailureThenUnitEvent, Unit>>(s => s.GetRequiredService<FirstAndSecondFailureThenUnitHandler>())
            .BuildServiceProvider();

        IEnumerable<IHostedService> backgroundEventListener = provider.GetServices<IHostedService>();
        InMemoryEventQueue eventQueue = (InMemoryEventQueue)provider.GetRequiredService<IEventQueueWriter>();

        FirstAndSecondFailureThenUnitEvent event2 = new();

        foreach (IHostedService service in backgroundEventListener)
        {
            _ = service.StartAsync(default);
        }

        await Task.Delay(1000);

        await eventQueue.EnqueueAsync(event2, default);

        FirstAndSecondFailureThenUnitHandler handler = provider.GetRequiredService<FirstAndSecondFailureThenUnitHandler>();

        handler.HandledEvent.WaitOne(5000).Should().BeTrue();
    }

    [Fact]
    public async Task InMemoryEventFlow_ThirdTry()
    {
        ILogger<EventListenerBackgroundTask> logger = Mock.Of<ILogger<EventListenerBackgroundTask>>();
        Mock<ILogger<EventListenerBackgroundTask>>? loggerMock = Mock.Get(logger);

        ServiceProvider provider = new ServiceCollection()
            .AddEventListener()
            .AddHangfireTaskListener(config => config.UseInMemoryStorage())
            .AddInMemoryEventQueue()
            .AddReflectionEventRunner()
            .AddScoped(_ => logger)
            .AddSingleton<AlwaysFailureHandler>()
            .AddScoped<IHandler<AlwaysFailureEvent, Unit>>(s => s.GetRequiredService<AlwaysFailureHandler>())
            .BuildServiceProvider();

        IEnumerable<IHostedService> backgroundEventListener = provider.GetServices<IHostedService>();
        InMemoryEventQueue eventQueue = (InMemoryEventQueue)provider.GetRequiredService<IEventQueueWriter>();

        AlwaysFailureEvent event2 = new();

        foreach (IHostedService service in backgroundEventListener)
        {
            _ = service.StartAsync(default);
        }
        await Task.Delay(1000);

        await eventQueue.EnqueueAsync(event2, default);

        await Task.Delay(5000);

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