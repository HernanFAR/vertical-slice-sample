using FluentAssertions;
using VSlices.Base.Responses;
namespace VSlices.Base.UnitTests;

public class BusinessFailureTests
{
    [Fact]
    public void Failure_ShouldReturnAddValues()
    {
        const string expTitle = "Title";
        const string expDetail = "Detail";
        const string expErrorName = "ErrorName";
        const string expErrorDetail = "ErrorDetail";
        var expErrors = new[] { new ValidationError(expErrorName, expErrorDetail) };
        var expCustomExtensions = new Dictionary<string, object?> { { "key", "value" } };

        var bus = new Failure
        {
            Kind = FailureKind.ValidationError,
            Title = expTitle,
            Detail = expDetail,
            Errors = expErrors,
            CustomExtensions = expCustomExtensions
        };

        bus.Kind.Should().Be(FailureKind.ValidationError);
        bus.Title.Should().Be(expTitle);
        bus.Detail.Should().Be(expDetail);
        bus.Errors.Should().BeEquivalentTo(expErrors);
        bus.CustomExtensions.Should().BeEquivalentTo(expCustomExtensions);
    }
}
