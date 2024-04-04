using FluentAssertions;
using FluentValidation;
using Moq;
using System.Diagnostics;
using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.CrossCutting.Pipeline.FluentValidation.UnitTests;

public class AbstractExceptionHandlingBehaviorTests
{
    public record RequestResult;
    
    public record Request(string Value) : IFeature<RequestResult>;

    public class Validator : AbstractValidator<Request>
    {
        public const string ValueEmptyMessage = "Test message";

        public Validator()
        {
            RuleFor(e => e.Value).NotEmpty().WithMessage(ValueEmptyMessage);
        }
    }

    [Fact]
    public async Task BeforeHandleAsync_ShouldInterrumptExecution()
    {
        const int expErrorCount = 1;
        var pipeline = new FluentValidationBehavior<Request, RequestResult>(new Validator());
        var request = new Request(null!);

        RequestHandlerDelegate<RequestResult> next = () => throw new UnreachableException();

        var result = await pipeline.HandleAsync(request, next, default);

        result.Failure.Errors.Should().HaveCount(expErrorCount);
        result.Failure.Errors[0].Name.Should().Be(nameof(Request.Value));
        result.Failure.Errors[0].Detail.Should().Be(Validator.ValueEmptyMessage);
        result.Failure.Kind.Should().Be(FailureKind.ValidationError);

    }

}