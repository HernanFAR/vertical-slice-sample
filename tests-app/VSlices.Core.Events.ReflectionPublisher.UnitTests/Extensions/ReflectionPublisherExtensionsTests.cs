using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Events.Strategies;

namespace VSlices.Core.Events._ReflectionPublisher.UnitTests.Extensions;

public class ReflectionPublisherExtensionsTests
{
    [Fact]
    public void AddReflectionPublisher_ShouldAddReflectionPublisher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddReflectionPublisher();

        // Assert
        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IPublisher>();
        var strategy = provider.GetRequiredService<IPublishingStrategy>();

        publisher.Should().BeOfType<ReflectionPublisher>();
        strategy.Should().BeOfType<AwaitInParallelStrategy>();
    }

    [Fact]
    public void AddReflectionPublisher_ShouldAddReflectionPublisher_WithDifferentStrategy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddReflectionPublisher(new AwaitForEachStrategy());

        // Assert
        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IPublisher>();
        var strategy = provider.GetRequiredService<IPublishingStrategy>();

        publisher.Should().BeOfType<ReflectionPublisher>();
        strategy.Should().BeOfType<AwaitForEachStrategy>();
    }
}
