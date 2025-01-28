using FluentAssertions;
using VSlices.Base.Failures;

namespace VSlices.Base.UnitTests;

public class FailureTests
{
    [Fact]
    public void Failure_ShouldReturnInstance_DetailDefault()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpected bus = ExtensibleExpected.BadRequest(expMessage, expCustomExtensions);

        bus.Code.Should().Be(400);
        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailNotAuthenticated()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpected bus = ExtensibleExpected.Unauthenticated(expMessage, expCustomExtensions);

        bus.Code.Should().Be(401);
        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailNotAuthorized()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpected bus = ExtensibleExpected.Forbidden(expMessage, expCustomExtensions);

        bus.Code.Should().Be(403);
        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailNotFound()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpected bus = ExtensibleExpected.NotFound(expMessage, expCustomExtensions);

        bus.Code.Should().Be(404);
        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailConflict()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpected bus = ExtensibleExpected.Conflict(expMessage, expCustomExtensions);

        bus.Code.Should().Be(409);  
        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailGone()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpected bus = ExtensibleExpected.Gone(expMessage, expCustomExtensions);

        bus.Code.Should().Be(410);  
        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailIAmTeapot()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpected bus = ExtensibleExpected.IAmTeapot(expMessage, expCustomExtensions);

        bus.Code.Should().Be(418);  
        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailUnprocessable()
    {
        const string expMessage = "Title";
        ValidationDetail[] expDetails = new [] { new ValidationDetail("Name", "Detail") };
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpected bus = ExtensibleExpected.Unprocessable(expMessage, expDetails, expCustomExtensions);

        bus.Code.Should().Be(422);  
        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailLocked()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpected bus = ExtensibleExpected.Locked(expMessage, expCustomExtensions);

        bus.Code.Should().Be(423);  
        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailFailedDependency()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpected bus = ExtensibleExpected.FailedDependency(expMessage, expCustomExtensions);

        bus.Code.Should().Be(424);  
        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailTooEarly()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpected bus = ExtensibleExpected.TooEarly(expMessage, expCustomExtensions);

        bus.Code.Should().Be(425);  
        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }
}
