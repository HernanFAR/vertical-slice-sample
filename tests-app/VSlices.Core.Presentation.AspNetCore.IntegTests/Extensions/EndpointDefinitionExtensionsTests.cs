using FluentAssertions;
using LanguageExt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Core;
using VSlices.Base.Definitions;

namespace VSlices.Core.Presentation.AspNetCore.IntegTests.Extensions;

public class EndpointDefinitionExtensionsTests
{
    public sealed record Input;

    public class Behavior : IBehavior<Input>
    {
        public Eff<VSlicesRuntime, Unit> Define(Input input) => throw new NotImplementedException();
    }

    public class EndpointIntegrator : IEndpointIntegrator, IFeatureDefinition
    {
        public void Define(IEndpointRouteBuilder builder)
        {
            builder.MapGet("api/test", Test);
        }

        public static IResult Test() => EmptyHttpResult.Instance;
        
        public static Unit Define(FeatureComposer feature) => 
            feature.With<Input>().ExpectNoOutput().ByExecuting<Behavior>()
                   .AndBindTo<EndpointIntegrator>();
    }

    public sealed record Feature2;

    public class Behavior2 : IBehavior<Feature2>
    {
        public Eff<VSlicesRuntime, Unit> Define(Feature2 input) => throw new NotImplementedException();
    }

    public class Endpoint2 : IEndpointIntegrator, IFeatureDefinition
    {
        public void Define(IEndpointRouteBuilder builder)
        {
            builder.MapGet("api/test2", Test);
        }

        public static IResult Test() => EmptyHttpResult.Instance;

        public static Unit Define(FeatureComposer feature) =>
            feature.With<Feature2>().ExpectNoOutput().ByExecuting<Behavior2>()
                   .AndBindTo<Endpoint2>();
    }

    [Fact]
    public void AddEndpointDefinition_ShouldAddSimpleEndpointAndDependencies()
    {
        var services = new ServiceCollection();

        EndpointIntegrator.Define(new FeatureComposer(services));

        services
            .Where(e => e.ServiceType == typeof(IIntegrator))
            .Where(e => e.ImplementationType == typeof(EndpointIntegrator))
            .Any(e => e.Lifetime == ServiceLifetime.Singleton)
            .Should().BeTrue();

    }
}
