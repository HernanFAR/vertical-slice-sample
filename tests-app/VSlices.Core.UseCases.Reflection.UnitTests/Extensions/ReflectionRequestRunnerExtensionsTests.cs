using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace VSlices.Core.UseCases.Reflection.UnitTests.Extensions;

public class ReflectionRequestRunnerExtensionsTests
{
    [Fact]
    public void AddReflectionPublisher_ShouldAddReflectionPublisher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddReflectionRequestRunner();

        // Assert
        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IRequestRunner>();

        sender.Should().NotBeNull();
    }
}
