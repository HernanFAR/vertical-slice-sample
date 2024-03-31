using FluentAssertions;
using Moq;
using VSlices.Core.Abstracts.Requests;

namespace VSlices.Core.Events.EventQueue.InMemory.UnitTests;

public class InMemoryEventQueueTests
{
    [Fact]
    public async Task EnqueueAsync_ShouldEnqueueEvent()
    {
        // Arrange
        const int expCount = 1;
        var options = new InMemoryEventQueueConfiguration
        {
            Capacity = 3
        };

        var eventMock = Mock.Of<IEvent>();
        var inMemoryEventQueue = new InMemoryEventQueue(options);


        // Act
        await inMemoryEventQueue.EnqueueAsync(eventMock, CancellationToken.None);


        // Assert
        inMemoryEventQueue._channel.Reader.Count.Should().Be(expCount);

        var item = await inMemoryEventQueue._channel.Reader.ReadAsync(CancellationToken.None);
        item.Should().Be(eventMock);

        inMemoryEventQueue._channel.Reader.Count.Should().Be(expCount - 1);
    }

    [Fact]
    public async Task DequeueAsync_ShouldEnqueueEvent()
    {
        // Arrange
        const int expCount = 0;
        var options = new InMemoryEventQueueConfiguration
        {
            Capacity = 3
        };

        var eventMock = Mock.Of<IEvent>();
        var inMemoryEventQueue = new InMemoryEventQueue(options);

        await inMemoryEventQueue._channel.Writer.WriteAsync(eventMock, CancellationToken.None);

        // Act
        var item = await inMemoryEventQueue.DequeueAsync(CancellationToken.None);


        // Assert
        inMemoryEventQueue._channel.Reader.Count.Should().Be(expCount);
        item.Should().Be(eventMock);

        
    }
}