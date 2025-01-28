using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Extensions;
using VSlices.Base.Traits;

namespace VSlices.Base.UnitTests.Extensions;

public sealed class DependencyProviderExtensionsTests
{
    public sealed class Dependency;

    [Fact]
    public void Provide_Success_ShouldReturnService()
    {
        // Arrange
        ServiceProvider services = new ServiceCollection()
                                   .AddTransient<Dependency>()
                                   .BuildServiceProvider();

        var provider = new DependencyProvider(services);

        Eff<VSlicesRuntime, Dependency> eff = DependencyProviderExtensions<Eff<VSlicesRuntime>, VSlicesRuntime>.Provide<Dependency>().As();

        var runtime = VSlicesRuntime.New(provider);

        // Act
        Fin<Dependency> result = eff.Run(runtime, EnvIO.New());

        // Arrange
        result.IsSucc.Should().BeTrue();

        _ = result.Map(e => e.Should().BeOfType<Dependency>());
    }

    [Fact]
    public void ProvideOptional_Success_ShouldReturnService()
    {
        // Arrange
        ServiceProvider services = new ServiceCollection()
                                   .AddTransient<Dependency>()
                                   .BuildServiceProvider();

        var provider = new DependencyProvider(services);

        Eff<VSlicesRuntime, Option<Dependency>> eff = DependencyProviderExtensions<Eff<VSlicesRuntime>, VSlicesRuntime>.ProvideOptional<Dependency>().As();

        var runtime = VSlicesRuntime.New(provider);

        // Act
        Fin<Option<Dependency>> result = eff.Run(runtime, EnvIO.New());

        // Arrange
        result.IsSucc.Should().BeTrue();

        _ = result.Map(e => e.Case.Should().BeOfType<Dependency>());
    }

    [Fact]
    public void ProvideOptional_Success_ShouldReturnNull()
    {
        // Arrange
        ServiceProvider services = new ServiceCollection().BuildServiceProvider();

        var provider = new DependencyProvider(services);

        Eff<VSlicesRuntime, Option<Dependency>> eff = DependencyProviderExtensions<Eff<VSlicesRuntime>, VSlicesRuntime>.ProvideOptional<Dependency>().As();

        var runtime = VSlicesRuntime.New(provider);

        // Act
        Fin<Option<Dependency>> result = eff.Run(runtime, EnvIO.New());

        // Arrange
        result.IsSucc.Should().BeTrue();

        _ = result.Map(e => e.Case.Should().BeNull());
    }
}
