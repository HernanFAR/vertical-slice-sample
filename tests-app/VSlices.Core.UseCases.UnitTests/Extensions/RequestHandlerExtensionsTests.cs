using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using VSlices.Base;
using VSlices.Core.Builder;

namespace VSlices.Core.UseCases.UnitTests.Extensions;

public class RequestHandlerExtensionsTests
{
    public record Feature1 : IRequest<Unit>;
    
    public class Handler1 : IRequestHandler<Feature1>
    {
        public Eff<VSlicesRuntime, Unit> Define(Feature1 request)
        {
            throw new UnreachableException();
        }
    }

    public record Response2 { }
    public record Feature2 : IRequest<Response2>;
    public class Handler2 : IRequestHandler<Feature2, Response2>
    {
        public Eff<VSlicesRuntime, Response2> Define(Feature2 request)
        {
            throw new UnreachableException();
        }
    }

    [Fact]
    public void AddHandlersFrom_ShouldAdHandlerImplementationsUsingTwoGenericOverload()
    {
        // Arrange
        var featureBuilder = new FeatureBuilder(new ServiceCollection());

        // Act
        featureBuilder.AddRequestHandler<Handler1>();
        featureBuilder.AddRequestHandler<Handler2>();


        // Assert
        featureBuilder.Services
            .Where(e => e.ImplementationType == typeof(Handler1))
            .Any(e => e.ServiceType == typeof(IRequestHandler<Feature1, Unit>))
            .Should().BeTrue();

        featureBuilder.Services
            .Where(e => e.ImplementationType == typeof(Handler2))
            .Any(e => e.ServiceType == typeof(IRequestHandler<Feature2, Response2>))
            .Should().BeTrue();

    }

    [Fact]
    public void AddHandlersFrom_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var featureBuilder = new FeatureBuilder(new ServiceCollection());
        var expMessage = $"The type {typeof(object).FullName} does not implement {typeof(IRequestHandler<,>).FullName}";

        // Act
        var act = () => featureBuilder.AddRequestHandler<object>();


        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);
        featureBuilder.Services.Should().BeEmpty();

    }
}
