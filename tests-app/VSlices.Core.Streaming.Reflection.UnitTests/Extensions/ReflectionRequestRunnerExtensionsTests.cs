using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Stream;

namespace VSlices.Core.UseCases.Reflection.UnitTests.Extensions;

public class ReflectionRequestRunnerExtensionsTests
{
    [Fact]
    public void AddReflectionPublisher_ShouldAddReflectionPublisher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddReflectionStreamRunner();

        // Assert
        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IStreamRunner>();

        sender.Should().NotBeNull();
    }
}
