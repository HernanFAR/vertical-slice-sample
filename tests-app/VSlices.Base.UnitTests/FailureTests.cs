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

        ExtensibleExpectedError bus = new BadRequest(expMessage, expCustomExtensions);

        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailNotAuthenticated()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpectedError bus = new Unauthenticated(expMessage, expCustomExtensions);

        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailNotAuthorized()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpectedError bus = new Forbidden(expMessage, expCustomExtensions);

        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailNotFound()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpectedError bus = new NotFound(expMessage, expCustomExtensions);

        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailConflict()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpectedError bus = new Conflict(expMessage, expCustomExtensions);

        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailGone()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpectedError bus = new Gone(expMessage, expCustomExtensions);

        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailIAmTeapot()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpectedError bus = new IAmTeapot(expMessage, expCustomExtensions);

        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailUnprocessable()
    {
        const string expMessage = "Title";
        ValidationDetail[] expDetails = new [] { new ValidationDetail("Name", "Detail") };
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpectedError bus = new Unprocessable(expMessage, expDetails, expCustomExtensions);

        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailLocked()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpectedError bus = new Locked(expMessage, expCustomExtensions);

        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailFailedDependency()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpectedError bus = new FailedDependency(expMessage, expCustomExtensions);

        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }

    [Fact]
    public void Failure_ShouldReturnInstance_DetailTooEarly()
    {
        const string expMessage = "Title";
        Dictionary<string, object?> expCustomExtensions = new() { { "key", "value" } };

        ExtensibleExpectedError bus = new TooEarly(expMessage, expCustomExtensions);

        bus.Message.Should().Be(expMessage);
        bus.Extensions.Should().BeEquivalentTo(expCustomExtensions);
    }
}
