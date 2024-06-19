using FluentAssertions;
using FluentValidation;
using System.Diagnostics;
using LanguageExt;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Base.Failures;

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
        FluentValidationBehavior<Request, RequestResult> pipeline = new(new Validator());
        Request request = new(null!);

        Aff<RequestResult> next = Aff<RequestResult>(() => throw new UnreachableException());

        Aff<RequestResult> pipelineEffect = pipeline.Define(request, next, default);

        Fin<RequestResult> pipelineResult = await pipelineEffect.Run();

        _ = pipelineResult
            .Match(
                _ => throw new UnreachableException(),
                failure =>
                {
                    var unprocessable = (Unprocessable)failure;

                    unprocessable.Errors.Should().HaveCount(expErrorCount);
                    unprocessable.Errors[0].Name.Should().Be(nameof(Request.Value));
                    unprocessable.Errors[0].Detail.Should().Be(Validator.ValueEmptyMessage);

                    return unit;
                }
            );
    }
}