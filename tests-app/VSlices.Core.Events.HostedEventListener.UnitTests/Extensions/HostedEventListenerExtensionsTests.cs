using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VSlices.Core.Events.Configurations;

namespace VSlices.Core.Events._HostedEventListener.UnitTests.Extensions;

public class HostedEventListenerExtensionsTests
{
    [Fact]
    public void AddDefaultHostedEventListener_ShouldAddEventListener_DetailWithoutConfig()
    {
        var services = new ServiceCollection();

        services.AddDefaultHostedEventListener();

        services.Where(e => e.ServiceType == typeof(IEventListenerCore))
            .Where(e => e.ImplementationType == typeof(EventListenerCore))
            .Any(e => e.Lifetime == ServiceLifetime.Singleton)
            .Should().BeTrue();

        services.Where(e => e.ServiceType == typeof(IHostedService))
            .Where(e => e.ImplementationType == typeof(HostedEventListener))
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
    public void AddDefaultHostedEventListener_ShouldAddEventListener_DetailWithConfig()
    {
        const MoveActions moveActions = MoveActions.ImmediateRetry;
        var services = new ServiceCollection();

        services.AddDefaultHostedEventListener(config =>
        {
            config.ActionInException = moveActions;
        });

        services.Where(e => e.ServiceType == typeof(IEventListenerCore))
            .Where(e => e.ImplementationType == typeof(EventListenerCore))
            .Any(e => e.Lifetime == ServiceLifetime.Singleton)
            .Should().BeTrue();

        services.Where(e => e.ServiceType == typeof(IHostedService))
            .Where(e => e.ImplementationType == typeof(HostedEventListener))
            .Any(e => e.Lifetime == ServiceLifetime.Singleton)
            .Should().BeTrue();

        var descriptor = services
            .Where(e => e.ServiceType == typeof(EventListenerConfiguration))
            .Single(e => e.Lifetime == ServiceLifetime.Singleton);

        var opts = (EventListenerConfiguration)descriptor.ImplementationInstance!;

        opts.ActionInException.Should().Be(moveActions);
        opts.MaxRetries.Should().Be(3);


    }

}
