using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Events._InMemoryEventQueue.UnitTests.Extensions;

public class InMemoryEventQueueExtensionsTests
{
    [Fact]
    public void AddInMemoryEventQueue_ShouldAddServiceDescription_DetailWithoutConfigAction()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInMemoryEventQueue();

        // Assert
        services
            .Where(e => e.ServiceType == typeof(IEventQueue))
            .Any(e => e.ImplementationType == typeof(InMemoryEventQueue))
            .Should().BeTrue();

        InMemoryEventQueueConfiguration? config = services
            .Single(e => e.ServiceType == typeof(InMemoryEventQueueConfiguration))
            .ImplementationInstance as InMemoryEventQueueConfiguration;

        config.Should().NotBeNull();

        config!.Capacity.Should().Be(50);

    }

    [Fact]
    public void AddInMemoryEventQueue_ShouldAddServiceDescription_DetailWithConfigAction()
    {
        // Arrange
        const int capacity = 10;
        var services = new ServiceCollection();

        // Act
        services.AddInMemoryEventQueue(config =>
        {
            config.Capacity = capacity;
        });

        // Assert
        services
            .Where(e => e.ServiceType == typeof(IEventQueue))
            .Any(e => e.ImplementationType == typeof(InMemoryEventQueue))
            .Should().BeTrue();

        InMemoryEventQueueConfiguration? config = services
            .Single(e => e.ServiceType == typeof(InMemoryEventQueueConfiguration))
            .ImplementationInstance as InMemoryEventQueueConfiguration;

        config.Should().NotBeNull();

        config!.Capacity.Should().Be(capacity);

    }
}
