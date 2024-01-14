using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Responses;

namespace VSlices.Core.UnitTests.Extensions;

public class FeatureDependencyExtensionsTests
{
    public class DependencyDefinition1 : IFeatureDependencyDefinition
    {
        public class A { }
        public class B { }

        public static void DefineDependencies(IServiceCollection services)
        {
            services.AddTransient<A>();
            services.AddTransient<B>();
        }
    }

    public class DependencyDefinition2 : IFeatureDependencyDefinition
    {
        public class C { }

        public static void DefineDependencies(IServiceCollection services)
        {
            services.AddTransient<C>();
        }
    }

    [Fact]
    public void AddFeatureDependenciesFromAssemblyContaining_ShouldAddDependenciesInDependencyDefinitions()
    {
        var services = new ServiceCollection();

        services.AddFeatureDependenciesFromAssemblyContaining<Anchor>();

        services.Any(e => e.ImplementationType == typeof(DependencyDefinition1.A))
            .Should().BeTrue();
        services.Any(e => e.ImplementationType == typeof(DependencyDefinition1.B))
            .Should().BeTrue();
        services.Any(e => e.ImplementationType == typeof(DependencyDefinition2.C))
            .Should().BeTrue();
    }

    [Fact]
    public void AddFeatureDependency_ShouldAddDependenciesInDependencyDefinitions()
    {
        var services = new ServiceCollection();

        services.AddFeatureDependency<DependencyDefinition1>();

        services.Any(e => e.ImplementationType == typeof(DependencyDefinition1.A))
            .Should().BeTrue();
        services.Any(e => e.ImplementationType == typeof(DependencyDefinition1.B))
            .Should().BeTrue();
    }

    [Fact]
    public void AddFeatureDependency_ShouldAddDependenciesInDependencyDefinitions_Detail_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();

        var act = () => services.AddFeatureDependency(typeof(Success));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"{typeof(Success).FullName} does not implement {nameof(IFeatureDependencyDefinition)}");
    }

    public record Anchor;
}
