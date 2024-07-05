using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Events.Strategies;

namespace VSlices.Core.Events._ReflectionRunner.UnitTests.Extensions;

public class ReflectionRunnerExtensionsTests
{
    [Fact]
    public void AddReflectionPublisher_ShouldAddReflectionPublisher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddReflectionEventRunner();

        // Assert
        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IEventRunner>();
        var strategy = provider.GetRequiredService<IPublishingStrategy>();

        publisher.Should().BeOfType<ReflectionEventRunner>();
        strategy.Should().BeOfType<AwaitInParallelStrategy>();
    }

    [Fact]
    public void AddReflectionPublisher_ShouldAddReflectionPublisher_WithDifferentStrategy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddReflectionEventRunner(new AwaitForEachStrategy());

        // Assert
        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IEventRunner>();
        var strategy = provider.GetRequiredService<IPublishingStrategy>();

        publisher.Should().BeOfType<ReflectionEventRunner>();
        strategy.Should().BeOfType<AwaitForEachStrategy>();
    }
}
