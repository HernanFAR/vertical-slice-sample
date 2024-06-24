using FluentAssertions;
using FluentValidation;
using System.Diagnostics;
using LanguageExt;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Core.Stream;

namespace VSlices.CrossCutting.StreamPipeline.FluentValidation.UnitTests;

public class AbstractExceptionHandlingBehaviorTests
{
    public record Result(string Value);

    public record Request(string Value) : IStream<Result>;

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
        FluentValidationStreamBehavior<Request, Result> pipeline = new(new Validator());
        Request request = new(null!);

        Aff<IAsyncEnumerable<Result>> next = Aff<IAsyncEnumerable<Result>>(() => throw new UnreachableException());

        Aff<IAsyncEnumerable<Result>> pipelineEffect = pipeline.Define(request, next, default);

        Fin<IAsyncEnumerable<Result>> pipelineResult = await pipelineEffect.Run();

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


    [Fact]
    public async Task BeforeHandleAsync_ShouldProcess()
    {
        const string expResultMessage = "testing :D";
        FluentValidationStreamBehavior<Request, Result> pipeline = new(new Validator());
        Request request = new(expResultMessage);

        async IAsyncEnumerable<Result> Yield()
        {
            yield return new Result(expResultMessage);
        }

        Aff<IAsyncEnumerable<Result>> next = Eff(Yield);

        Aff<IAsyncEnumerable<Result>> pipelineEffect = pipeline.Define(request, next, default);

        Fin<IAsyncEnumerable<Result>> pipelineResult = await pipelineEffect.Run();

        await pipelineResult
            .Match(
                async enumeration =>
                {
                    await foreach (Result val in enumeration)
                    {
                        val.Value.Should().Be(expResultMessage);
                    }
                }, 
                _ => throw new UnreachableException()
            );
    }

}