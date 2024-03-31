using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.UseCases;

namespace VSlices.Core.Sender.Reflection.IntegTests.Extensions;

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
