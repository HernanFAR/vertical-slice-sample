using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using VSlices.Core.Builder;

namespace VSlices.Core.Stream.UnitTests.Extensions;

public class StreamHandlerExtensionsTests
{
    public record Result;

    public record Feature : IStream<Result>;
    
    public class Handler : IStreamHandler<Feature, Result>
    {
        public Eff<HandlerRuntime, IAsyncEnumerable<Result>> Define(Feature request)
        {
            throw new UnreachableException();
        }
    }

    [Fact]
    public void AddStreamHandler_ShouldAddHandlerImplementationsUsingTwoGenericOverload()
    {
        // Arrange
        var featureBuilder = new FeatureBuilder(new ServiceCollection());

        // Act
        featureBuilder.AddStreamHandler<Handler>();

        // Assert
        featureBuilder.Services
            .Where(e => e.ImplementationType == typeof(Handler))
            .Any(e => e.ServiceType == typeof(IStreamHandler<Feature, Result>))
            .Should().BeTrue();
        
    }

    [Fact]
    public void AddStreamHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var featureBuilder = new FeatureBuilder(new ServiceCollection());
        var expMessage = $"The type {typeof(object).FullName} does not implement {typeof(IStreamHandler<,>).FullName}";

        // Act
        var act = () => featureBuilder.AddStreamHandler<object>();


        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);
        featureBuilder.Services.Should().BeEmpty();

    }
}
