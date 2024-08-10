using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Failures;
using VSlices.Base.Traits;
using static VSlices.VSlicesPrelude;

namespace VSlices.Base.UnitTests;

public sealed class VSlicesPreludeTests
{
    public sealed class Dependency;

    [Fact]
    public void provide_Success_ShouldReturnServiceInstance()
    {
        // Arrange
        var services = new ServiceCollection()
                       .AddTransient<Dependency>()
                       .BuildServiceProvider();

        var provider = new DependencyProvider(services);    

        var effect = from dependency in provide<Dependency>()
                      select dependency;

        // Act
        var result = effect.Run(VSlicesRuntime.New(provider), EnvIO.New());

        // Assert
        result.IsSucc.Should().BeTrue();
        result.Map(d => d.Should().BeOfType<Dependency>());
    }

    [Fact]
    public void provideOptional_Success_ShouldReturnServiceInstance()
    {
        // Arrange
        var services = new ServiceCollection()
                       .AddTransient<Dependency>()
                       .BuildServiceProvider();

        var provider = new DependencyProvider(services);    

        var effect = from dependency in provideOptional<Dependency>()
                      select dependency;

        // Act
        var result = effect.Run(VSlicesRuntime.New(provider), EnvIO.New());

        // Assert
        result.IsSucc.Should().BeTrue();
        result.IsFail.Should().BeFalse();

        result.Map(d => d.Case.Should().BeOfType<Dependency>());
    }

    [Fact]
    public void provideOptional_Success_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection().BuildServiceProvider();

        var provider = new DependencyProvider(services);    

        var effect = from dependency in provideOptional<Dependency>()
                      select dependency;

        // Act
        var result = effect.Run(VSlicesRuntime.New(provider), EnvIO.New());

        // Assert
        result.IsSucc.Should().BeTrue();
        result.IsFail.Should().BeFalse();
        result.Map(d => d.Case.Should().BeNull());
    }

    public static IEnumerable<object[]> ErrorTheoryData()
    {
        yield return ["BadRequest", new Dictionary<string, object?> { { "key", "value" } }];
        yield return ["Forbidden", new Dictionary<string, object?>()];
        yield return ["Gone", new Dictionary<string, object?>{ { "none", null } }];  
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void badRequest_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Act
        Error error = badRequest(message, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(400);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(extensions);   
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void unauthenticated_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Act
        Error error = unauthenticated(message, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(401);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(extensions);
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void forbidden_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Act
        Error error = forbidden(message, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(403);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(extensions);
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void notFound_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Act
        Error error = notFound(message, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(404);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(extensions);
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void conflict_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Act
        Error error = conflict(message, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(409);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(extensions);
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void gone_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Act
        Error error = gone(message, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(410);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(extensions);
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void iAmTeaPot_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Act
        Error error = iAmTeaPot(message, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(418);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(extensions);
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void unprocessable_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Assert
        ValidationDetail[]          errors        = [new ValidationDetail("key", "value")];
        Dictionary<string, object?> expExts = new(extensions)
        {
            ["errors"] = errors
                         .Select(x => x.Name)
                         .Distinct()
                         .ToDictionary(propertyName => propertyName,
                                       propertyName => errors
                                                       .Where(x => x.Name == propertyName)
                                                       .Select(e => e.Detail)
                                                       .ToArray())
        };

        // Act
        Error error = unprocessable(message, errors, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(422);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(expExts);
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void locked_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Act
        Error error = locked(message, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(423);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(extensions);
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void failedDependency_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Act
        Error error = failedDependency(message, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(424);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(extensions);
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void tooEarly_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Act
        Error error = tooEarly(message, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(425);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(extensions);
    }

    [Theory]
    [MemberData(nameof(ErrorTheoryData))]
    public void serverError_Success_ShouldReturnErrorInstance(string message, Dictionary<string, object?> extensions)
    {
        // Act
        Error error = serverError(message, extensions);

        // Assert
        var expected = (ExtensibleExpected)error;

        expected.Code.Should().Be(500);
        expected.Message.Should().Be(message);
        expected.Extensions.Should().BeEquivalentTo(extensions);
    }
}
