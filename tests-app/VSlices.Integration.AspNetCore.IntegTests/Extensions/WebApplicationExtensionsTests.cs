using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using VSlices.Core.Builder;
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

    public class EndpointDefinition : IEndpointDefinition
    {
        public const string ApiRoute = "api/test";

        public void Define(IEndpointRouteBuilder builder)
        {
            builder.MapGet(ApiRoute, Test);
        }

        public static Task Test(HttpContext context) => Task.FromResult<IResult>(EmptyHttpResult.Instance);

        public static void DefineDependencies(FeatureBuilder featureBuilder)
        {

        }
    }

    [Fact]
    public void UseEndpointDefinitions_ShouldCallMethods()
    {
        var featureBuilder = new FeatureBuilder(new ServiceCollection());

        featureBuilder.AddEndpoint<EndpointDefinition>();

        var provider = featureBuilder.Services.BuildServiceProvider();

        var webAppDummy = new WebAppDummy(provider);

        webAppDummy.UseEndpointDefinitions();

        var dataSources = webAppDummy.DataSources.First();

        var addedEndpoint = (RouteEndpoint)dataSources.Endpoints[0];

        if (addedEndpoint.RequestDelegate is null) throw new ArgumentNullException(nameof(addedEndpoint.RequestDelegate));

        addedEndpoint.RequestDelegate.Method.Should().BeSameAs(typeof(EndpointDefinition).GetMethod(nameof(EndpointDefinition.Test)));
        addedEndpoint.RoutePattern.RawText.Should().Be(EndpointDefinition.ApiRoute);
    }
}
