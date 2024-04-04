using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace VSlices.Core.UseCases.Reflection.UnitTests.Extensions;

public class ReflectionSenderExtensionsTests
{
    [Fact]
    public void AddReflectionPublisher_ShouldAddReflectionPublisher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddReflectionSender();

        // Assert
        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        sender.Should().NotBeNull();
    }
}
