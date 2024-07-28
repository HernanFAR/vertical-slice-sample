﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using VSlices.Base;
using VSlices.Core.Builder;

namespace VSlices.Core.UnitTests.Extensions;

public class HandlerExtensionsTests
{
    public record Feature1 : IFeature<Unit>;
    
    public class Handler1 : IHandler<Feature1>
    {
        public Eff<HandlerRuntime, Unit> Define(Feature1 request)
        {
            throw new UnreachableException();
        }
    }

    public record Response2 { }
    public record Feature2 : IFeature<Response2>;
    public class Handler2 : IHandler<Feature2, Response2>
    {
        public Eff<HandlerRuntime, Response2> Define(Feature2 request)
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
        featureBuilder.AddHandler<Handler1>();
        featureBuilder.AddHandler<Handler2>();


        // Assert
        featureBuilder.Services
            .Where(e => e.ImplementationType == typeof(Handler1))
            .Any(e => e.ServiceType == typeof(IHandler<Feature1, Unit>))
            .Should().BeTrue();

        featureBuilder.Services
            .Where(e => e.ImplementationType == typeof(Handler2))
            .Any(e => e.ServiceType == typeof(IHandler<Feature2, Response2>))
            .Should().BeTrue();

    }

    [Fact]
    public void AddHandlersFrom_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var featureBuilder = new FeatureBuilder(new ServiceCollection());
        var expMessage = $"The type {typeof(object).FullName} does not implement {typeof(IHandler<,>).FullName}";

        // Act
        var act = () => featureBuilder.AddHandler<object>();


        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage(expMessage);
        featureBuilder.Services.Should().BeEmpty();

    }
}
