using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using LanguageExt;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Core.Presentation;

namespace VSlices.Integration.AspNetCore.IntegTests.Extensions;

public class WebApplicationExtensionsTests
{
    public class WebAppDummy(IServiceProvider serviceProvider) : IEndpointRouteBuilder
    {
        public IApplicationBuilder CreateApplicationBuilder()
        {
            throw new NotImplementedException();
        }

        public IServiceProvider ServiceProvider { get; } = serviceProvider;
        public ICollection<EndpointDataSource> DataSources { get; } = new Collection<EndpointDataSource>();
    }

    public record Feature : IFeature<Unit>;

    public record Handler : IHandler<Feature>
    {
        public Eff<VSlicesRuntime, Unit> Define(Feature input)
        {
            throw new NotImplementedException();
        }
    }

    public class EndpointIntegrator : IEndpointIntegrator, IFeatureDependencies<Feature>
    {
        public const string ApiRoute = "api/test";

        public void Define(IEndpointRouteBuilder builder)
        {
            builder.MapGet(ApiRoute, Test);
        }

        public static Task Test(HttpContext context) => Task.FromResult<IResult>(EmptyHttpResult.Instance);
        public static void DefineDependencies(IFeatureStartBuilder<Feature, Unit> feature)
        {
            feature.FromIntegration.Using<EndpointIntegrator>()
                   .Execute<Handler>();
        }
    }

    [Fact]
    public void UseEndpointDefinitions_ShouldCallMethods()
    {
        var services = new ServiceCollection();

        EndpointIntegrator.DefineDependencies(new FeatureDefinition<Feature, Unit>(services));

        var provider = services.BuildServiceProvider();

        var webAppDummy = new WebAppDummy(provider);

        webAppDummy.UseEndpointDefinitions();

        var dataSources = webAppDummy.DataSources.First();

        var addedEndpoint = (RouteEndpoint)dataSources.Endpoints[0];

        if (addedEndpoint.RequestDelegate is null) throw new ArgumentNullException(nameof(addedEndpoint.RequestDelegate));

        addedEndpoint.RequestDelegate.Method.Should().BeSameAs(typeof(EndpointIntegrator).GetMethod(nameof(EndpointIntegrator.Test)));
        addedEndpoint.RoutePattern.RawText.Should().Be(EndpointIntegrator.ApiRoute);
    }
}
