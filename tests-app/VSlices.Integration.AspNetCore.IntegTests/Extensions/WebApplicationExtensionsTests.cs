using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using FluentAssertions;
using VSlices.Core.Presentation;

namespace VSlices.Integration.AspNetCore.IntegTests.Extensions;

public class WebApplicationExtensionsTests
{
    public class WebAppDummy : IEndpointRouteBuilder
    {
        public WebAppDummy(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            DataSources = new Collection<EndpointDataSource>();
        }

        public IApplicationBuilder CreateApplicationBuilder()
        {
            throw new NotImplementedException();
        }

        public IServiceProvider ServiceProvider { get; }
        public ICollection<EndpointDataSource> DataSources { get; }
    }

    public class Endpoint : IEndpointDefinition
    {
        public const string ApiRoute = "api/test";

        public void DefineEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet(ApiRoute, Test);
        }

        public static Task Test(HttpContext context) => Task.FromResult<IResult>(EmptyHttpResult.Instance);

        public static void DefineDependencies(IServiceCollection services)
        {

        }
    }

    [Fact]
    public void UseEndpointDefinitions_ShouldCallMethods()
    {
        var services = new ServiceCollection();

        services.AddEndpointDefinition<Endpoint>();

        var provider = services.BuildServiceProvider();

        var webAppDummy = new WebAppDummy(provider);

        webAppDummy.UseEndpointDefinitions();

        var dataSources = webAppDummy.DataSources.First();

        var addedEndpoint = (RouteEndpoint)dataSources.Endpoints[0];

        if (addedEndpoint.RequestDelegate is null) throw new ArgumentNullException(nameof(addedEndpoint.RequestDelegate));

        addedEndpoint.RequestDelegate.Method.Should().BeSameAs(typeof(Endpoint).GetMethod(nameof(Endpoint.Test)));
        addedEndpoint.RoutePattern.RawText.Should().Be(Endpoint.ApiRoute);
    }
}
