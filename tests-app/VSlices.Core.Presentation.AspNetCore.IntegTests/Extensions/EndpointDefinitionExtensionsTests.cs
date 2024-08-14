using FluentAssertions;
using LanguageExt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Builder;
using VSlices.Base.Core;

namespace VSlices.Core.Presentation.AspNetCore.IntegTests.Extensions;

public class EndpointDefinitionExtensionsTests
{
    public sealed record Feature : IFeature<Unit>;

    public class EndpointIntegrator : IEndpointIntegrator, IFeatureDependencies<Feature>
    {
        public void Define(IEndpointRouteBuilder builder)
        {
            builder.MapGet("api/test", Test);
        }

        public static IResult Test() => EmptyHttpResult.Instance;
        
        public static void DefineDependencies(IFeatureStartBuilder<Feature, Unit> feature)
        {
            feature.FromIntegration.Using<EndpointIntegrator>();
        }
    }

    public sealed record Feature2 : IFeature<Unit>;

    public class Endpoint2 : IEndpointIntegrator, IFeatureDependencies<Feature2>
    {
        public void Define(IEndpointRouteBuilder builder)
        {
            builder.MapGet("api/test2", Test);
        }

        public static IResult Test() => EmptyHttpResult.Instance;

        public static void DefineDependencies(IFeatureStartBuilder<Feature2, Unit> feature)
        {
            feature.FromIntegration.Using<Endpoint2>();
        }
    }

    [Fact]
    public void AddEndpointDefinition_ShouldAddSimpleEndpointAndDependencies()
    {
        var services = new ServiceCollection();

        EndpointIntegrator.DefineDependencies(new FeatureDefinition<Feature, Unit>(services));

        services
            .Where(e => e.ServiceType == typeof(IIntegrator))
            .Where(e => e.ImplementationType == typeof(EndpointIntegrator))
            .Any(e => e.Lifetime == ServiceLifetime.Singleton)
            .Should().BeTrue();

    }
}
