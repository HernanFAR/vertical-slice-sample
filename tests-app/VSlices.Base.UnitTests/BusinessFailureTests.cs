using FluentAssertions;
using VSlices.Base.Responses;
namespace VSlices.Base.UnitTests;

public class BusinessFailureTests
{
    [Fact]
    public void Failure_ShouldReturnAddValues()
    {
        const string expType = "Type";
        const string expTitle = "Title";
        const string expDetail = "Detail";
        const string expInstance = "Instance";
        const string expErrorName = "ErrorName";
        const string expErrorDetail = "ErrorDetail";
        var expErrors = new[] { new ValidationError(expErrorName, expErrorDetail) };
        var expCustomExtensions = new Dictionary<string, object?> { { "key", "value" } };

        var bus = new Failure
        {
            Kind = FailureKind.ValidationError,
            Type = expType,
            Title = expTitle,
            Detail = expDetail,
            Instance = expInstance,
            Errors = expErrors,
            CustomExtensions = expCustomExtensions
        };

        bus.Kind.Should().Be(FailureKind.ValidationError);
        bus.Type.Should().Be(expType);
        bus.Title.Should().Be(expTitle);
        bus.Detail.Should().Be(expDetail);
        bus.Instance.Should().Be(expInstance);
        bus.Errors.Should().BeEquivalentTo(expErrors);
        bus.CustomExtensions.Should().BeEquivalentTo(expCustomExtensions);
    }
}
