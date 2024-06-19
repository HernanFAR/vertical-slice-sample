using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Events.Configurations;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events.Units.Extensions;

public class EventExtensionsTests
{
    public class EventRunner : IEventRunner
    {
        public ValueTask<Fin<Unit>> PublishAsync(IEvent @event, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class EventQueue : IEventQueue
    {
        public ValueTask<IEvent> PeekAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IEvent> DequeueAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string BackgroundReaderProvider => throw new NotImplementedException();

        public ValueTask EnqueueAsync(IEvent @event, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string BackgroundWriterProvider => throw new NotImplementedException();
    }

    [Fact]
    public void AddPublisher_ShouldAddPublisher()
    {
        var services = new ServiceCollection();

        services.AddPublisher<EventRunner>();

        services
            .Where(e => e.ServiceType == typeof(IEventRunner))
            .Where(e => e.ImplementationType == typeof(EventRunner))
            .Any(e => e.Lifetime == ServiceLifetime.Scoped)
            .Should().BeTrue();

    }

    [Fact]
    public void AddPublisher_ShouldThrowException()
    {
        var expMessage = $"{typeof(object).FullName} does not implement {typeof(IEventRunner).FullName}";
        var services = new ServiceCollection();

        var act = () => services.AddPublisher(typeof(object));

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }

    [Fact]
    public void AddEventQueue_ShouldAddEventQueue()
    {
        var services = new ServiceCollection();

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
        var expMessage = $"{typeof(object).FullName} does not implement {typeof(IEventQueue).FullName}";
        var services = new ServiceCollection();

        var act = () => services.AddEventQueue(typeof(object));

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);

    }

    [Fact]
    public void AddDefaultEventListener_ShouldAddEventListener_DetailWithoutConfig()
    {
        var services = new ServiceCollection();

        services.AddEventListener<EventListenerCore>();

        services
            .Where(e => e.ServiceType == typeof(IEventListenerCore))
            .Where(e => e.ImplementationType == typeof(EventListenerCore))
            .Any(e => e.Lifetime == ServiceLifetime.Singleton)
            .Should().BeTrue();

        var descriptor = services
            .Where(e => e.ServiceType == typeof(EventListenerConfiguration))
            .Single(e => e.Lifetime == ServiceLifetime.Singleton);

        var opts = (EventListenerConfiguration)descriptor.ImplementationInstance!;

        opts.ActionInException.Should().Be(MoveActions.MoveLast);
        opts.MaxRetries.Should().Be(3);


    }

    [Fact]
    public void AddDefaultEventListener_ShouldAddEventListener_DetailWithConfig()
    {
        const MoveActions moveActions = MoveActions.ImmediateRetry;
        var services = new ServiceCollection();

        services.AddDefaultEventListener(config =>
        {
            config.ActionInException = moveActions;
        });

        services
            .Where(e => e.ServiceType == typeof(IEventListenerCore))
            .Where(e => e.ImplementationType == typeof(EventListenerCore))
            .Any(e => e.Lifetime == ServiceLifetime.Singleton)
            .Should().BeTrue();

        var descriptor = services
            .Where(e => e.ServiceType == typeof(EventListenerConfiguration))
            .Single(e => e.Lifetime == ServiceLifetime.Singleton);

        var opts = (EventListenerConfiguration)descriptor.ImplementationInstance!;

        opts.ActionInException.Should().Be(moveActions);
        opts.MaxRetries.Should().Be(3);


    }

    [Fact]
    public void AddDefaultEventListener_ShouldThrowInvalidOperationException()
    {
        var expMessage = $"{typeof(object).FullName} does not implement {typeof(IEventListenerCore).FullName}";
        var services = new ServiceCollection();

        var act = () => services.AddEventListener(typeof(object), null);

        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);


    }

}
