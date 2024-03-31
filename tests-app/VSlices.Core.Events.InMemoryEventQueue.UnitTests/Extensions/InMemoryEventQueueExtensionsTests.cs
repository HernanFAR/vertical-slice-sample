using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Abstracts.Events;

namespace VSlices.Core.Events.EventQueue.InMemory.UnitTests.Extensions;

public class InMemoryEventQueueExtensionsTests
{
    [Fact]
    public void AddInMemoryEventQueue_ShouldAddServiceDescription()
    {
        // Arrange
        var services = new ServiceCollection();


        // Act
        services.AddInMemoryEventQueue();


        // Assert
        services.Should().Contain(x => x.ServiceType == typeof(IEventQueue) && x.ImplementationType == typeof(InMemoryEventQueue));


    }
}
