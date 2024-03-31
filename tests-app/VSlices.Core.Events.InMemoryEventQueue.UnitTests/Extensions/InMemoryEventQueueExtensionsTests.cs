using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace VSlices.Core.Events.EventQueue.InMemory.UnitTests.Extensions;

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
        services.Where(e => e.ServiceType == typeof(IEventQueue))
            .Where(e => e.ImplementationType == typeof(InMemoryEventQueue))
            .Any()
            .Should().BeTrue();

        InMemoryEventQueueConfiguration? config = services
            .Where(e => e.ServiceType == typeof(InMemoryEventQueueConfiguration))
            .Single().ImplementationInstance as InMemoryEventQueueConfiguration;

        config.Should().NotBeNull();

        config.Capacity.Should().Be(50);


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
        services.Where(e => e.ServiceType == typeof(IEventQueue))
            .Where(e => e.ImplementationType == typeof(InMemoryEventQueue))
            .Any()
            .Should().BeTrue();
        
        InMemoryEventQueueConfiguration? config = services
            .Where(e => e.ServiceType == typeof(InMemoryEventQueueConfiguration))
            .Single().ImplementationInstance as InMemoryEventQueueConfiguration;

        config.Should().NotBeNull();

        config.Capacity.Should().Be(capacity);

    }
}
