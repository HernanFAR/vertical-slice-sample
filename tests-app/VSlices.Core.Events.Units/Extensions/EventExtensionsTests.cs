using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Events.Configurations;
using VSlices.CrossCutting.BackgroundTaskListener;
using VSlices.Domain.Interfaces;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Events.Units.Extensions;

public class EventExtensionsTests
{
    public class EventRunner : IEventRunner
    {
        public Fin<Unit> Publish(IEvent @event, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    public class EventQueue : IEventQueue
    {
        public ValueTask<IEvent> PeekAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        public ValueTask<IEvent> DequeueAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        public string BackgroundReaderProvider => throw new NotImplementedException();

        public ValueTask EnqueueAsync(IEvent @event, CancellationToken cancellationToken) => throw new NotImplementedException();

        public string BackgroundWriterProvider => throw new NotImplementedException();
    }

    [Fact]
    public void AddPublisher_ShouldAddPublisher()
    {
        ServiceCollection services = new();

        services.AddEventRunner<EventRunner>();

        services
            .Where(e => e.ServiceType == typeof(IEventRunner))
            .Where(e => e.ImplementationType == typeof(EventRunner))
            .Any(e => e.Lifetime == ServiceLifetime.Singleton)
            .Should().BeTrue();

    }

    [Fact]
    public void AddPublisher_ShouldThrowException()
    {
        string expMessage = $"{typeof(object).FullName} does not implement {typeof(IEventRunner).FullName}";
        ServiceCollection services = new();

        Func<IServiceCollection> act = () => services.AddEventRunner(typeof(object));

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }

    [Fact]
    public void AddEventQueue_ShouldAddEventQueue()
    {
        ServiceCollection services = new();

        services.AddEventQueue<EventQueue>();

        services
            .Where(e => e.ServiceType == typeof(IEventQueue))
            .Where(e => e.ImplementationType == typeof(EventQueue))
            .Any(e => e.Lifetime == ServiceLifetime.Singleton)
            .Should().BeTrue();

    }

    [Fact]
    public void AddEventQueue_ShouldThrowInvalidOperationException()
    {
        string expMessage = $"{typeof(object).FullName} does not implement {typeof(IEventQueue).FullName}";
        ServiceCollection services = new();

        Func<IServiceCollection> act = () => services.AddEventQueue(typeof(object));

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }
    [Fact]
    public void AddDefaultEventListener_ShouldAddEventListener_DetailWithConfig()
    {
        const MoveActions moveActions = MoveActions.ImmediateRetry;
        ServiceCollection services = new();

        services.AddEventListener(config =>
        {
            config.ActionInException = moveActions;
        });

        services
            .Where(e => e.ServiceType == typeof(IBackgroundTask))
            .Where(e => e.ImplementationType == typeof(EventListenerBackgroundTask))
            .Any(e => e.Lifetime == ServiceLifetime.Singleton)
            .Should().BeTrue();

        ServiceDescriptor descriptor = services
            .Where(e => e.ServiceType == typeof(EventListenerConfiguration))
            .Single(e => e.Lifetime == ServiceLifetime.Singleton);

        EventListenerConfiguration opts = (EventListenerConfiguration)descriptor.ImplementationInstance!;

        opts.ActionInException.Should().Be(moveActions);
        opts.MaxRetries.Should().Be(3);

    }
}
