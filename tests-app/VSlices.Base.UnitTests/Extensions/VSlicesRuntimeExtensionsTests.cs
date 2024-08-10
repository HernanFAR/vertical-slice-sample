using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace VSlices.Base.UnitTests.Extensions;

public sealed class VSlicesRuntimeExtensionsTests
{
    [Fact]
    public void AddVSlicesRuntime_Success_ShouldAddVSlicesRuntimeToPipeline()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddVSlicesRuntime();

        // Assert
        services.Where(e => e.Lifetime == ServiceLifetime.Scoped)
                .Any(e => e.ServiceType == typeof(VSlicesRuntime))
                .Should().BeTrue();
    }
}
