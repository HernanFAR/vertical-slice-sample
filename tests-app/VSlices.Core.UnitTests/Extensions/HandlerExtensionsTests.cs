using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.Core.UnitTests.Extensions;

public class HandlerExtensionsTests
{
    public record Feature1 : IFeature<Success>
    { }
    public class Handler1 : IHandler<Feature1>
    {
        public ValueTask<Result<Success>> HandleAsync(Feature1 request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public record Response2 { }
    public record Feature2 : IFeature<Response2> { }
    public class Handler2 : IHandler<Feature2, Response2>
    {
        public ValueTask<Result<Response2>> HandleAsync(Feature2 request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void AddHandlersFrom_ShouldAdHandlerImplementationsUsingTwoGenericOverload()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHandler<Handler1>();
        services.AddHandler<Handler2>();


        // Assert
        services
            .Where(e => e.ImplementationType == typeof(Handler1))
            .Any(e => e.ServiceType == typeof(IHandler<Feature1, Success>))
            .Should().BeTrue();

        services
            .Where(e => e.ImplementationType == typeof(Handler2))
            .Any(e => e.ServiceType == typeof(IHandler<Feature2, Response2>))
            .Should().BeTrue();

    }
}
