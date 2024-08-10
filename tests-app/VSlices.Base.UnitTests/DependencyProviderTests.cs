using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Traits;

namespace VSlices.Base.UnitTests;

public sealed class DependencyProviderTests
{
    public sealed class Dependency;

    [Fact]
    public void Provide_Success_ShouldReturnServiceInstance()
    {
        // Arrange
        var services = new ServiceCollection()
                       .AddTransient<Dependency>()
                       .BuildServiceProvider();

        var provider = new DependencyProvider(services);

        // Act
        var service = provider.Provide<Dependency>().runIO(EnvIO.New());

        // Assert 
        service.Should().BeOfType<Dependency>();
    }

    [Fact]
    public void ProvideOptional_Success_ShouldReturnServiceInstance()
    {
        // Arrange
        var services = new ServiceCollection()
                       .AddTransient<Dependency>()
                       .BuildServiceProvider();

        var provider = new DependencyProvider(services);

        // Act
        var optionalService = provider.ProvideOptional<Dependency>().runIO(EnvIO.New());

        // Assert 
        optionalService.IsNone.Should().BeFalse();
        optionalService.IsSome.Should().BeTrue();

        optionalService.Case.Should().BeOfType<Dependency>();
    }

    [Fact]
    public void ProvideOptional_Success_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection()
                       .BuildServiceProvider();

        var provider = new DependencyProvider(services);

        // Act
        var optionalService = provider.ProvideOptional<Dependency>().runIO(EnvIO.New());

        // Assert 
        optionalService.IsNone.Should().BeTrue();
        optionalService.IsSome.Should().BeFalse();

        optionalService.Case.Should().BeNull();
    }

}
