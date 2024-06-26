using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Builder;

namespace VSlices.Core.Presentation.AspNetCore.IntegTests.Extensions;

public class EndpointDefinitionExtensionsTests
{
    public class Dependency { }
    public class Dependency2 { }

    public class EndpointDefinition : IEndpointDefinition
    {
        public void Define(IEndpointRouteBuilder builder)
        {
            builder.MapGet("api/test", Test);
        }

        public static IResult Test() => EmptyHttpResult.Instance;

        public static void DefineDependencies(FeatureBuilder featureBuilder)
        {
            featureBuilder.Services.AddScoped<Dependency>();
        }
    }

    public class Endpoint2 : IEndpointDefinition
    {
        public void Define(IEndpointRouteBuilder builder)
        {
            builder.MapGet("api/test2", Test);
        }

        public static IResult Test() => EmptyHttpResult.Instance;

        public static void DefineDependencies(FeatureBuilder featureBuilder)
        {
            featureBuilder.Services.AddScoped<Dependency2>();
        }
    }

    [Fact]
    public void AddEndpointDefinition_ShouldAddSimpleEndpointAndDependencies()
    {
        var featureBuilder = new FeatureBuilder(new ServiceCollection());

        featureBuilder.AddEndpoint<EndpointDefinition>();

        featureBuilder.Services
            .Where(e => e.ServiceType == typeof(IEndpointDefinition))
            .Where(e => e.ImplementationType == typeof(EndpointDefinition))
            .Any(e => e.Lifetime == ServiceLifetime.Scoped)
            .Should().BeTrue();

    }
}
