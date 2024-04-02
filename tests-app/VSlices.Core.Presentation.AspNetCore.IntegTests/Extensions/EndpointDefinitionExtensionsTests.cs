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

    public class Endpoint : IEndpoint
    {
        public void DefineEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet("api/test", Test);
        }

        public static Task<IResult> Test(HttpContext context) => Task.FromResult<IResult>(EmptyHttpResult.Instance);

        public static void DefineDependencies(FeatureBuilder featureBuilder)
        {
            featureBuilder.Services.AddScoped<Dependency>();
        }
    }

    public class Endpoint2 : IEndpoint
    {
        public void DefineEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet("api/test2", Test);
        }

        public static Task<IResult> Test(HttpContext context) => Task.FromResult<IResult>(EmptyHttpResult.Instance);

        public static void DefineDependencies(FeatureBuilder featureBuilder)
        {
            featureBuilder.Services.AddScoped<Dependency2>();
        }
    }

    [Fact]
    public void AddEndpointDefinition_ShouldAddSimpleEndpointAndDependencies()
    {
        var featureBuilder = new FeatureBuilder(new ServiceCollection());

        featureBuilder.AddEndpoint<Endpoint>();

        featureBuilder.Services
            .Where(e => e.ServiceType == typeof(ISimpleEndpoint))
            .Where(e => e.ImplementationType == typeof(Endpoint))
            .Any(e => e.Lifetime == ServiceLifetime.Scoped)
            .Should().BeTrue();

    }
}
