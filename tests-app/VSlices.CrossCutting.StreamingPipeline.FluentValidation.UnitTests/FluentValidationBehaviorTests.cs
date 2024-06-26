using FluentAssertions;
using FluentValidation;
using System.Diagnostics;
using LanguageExt;
using LanguageExt.SysX.Live;
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
    public async Task BeforeHandleAsync_ShouldInterruptExecution()
    {
        const int expErrorCount = 1;
        FluentValidationStreamBehavior<Request, Result> pipeline = new(new Validator());
        Request request = new(null!);

        Aff<Runtime, IAsyncEnumerable<Result>> next = Aff<IAsyncEnumerable<Result>>(() => throw new UnreachableException());

        Aff<Runtime, IAsyncEnumerable<Result>> pipelineEffect = pipeline.Define(request, next);

        Fin<IAsyncEnumerable<Result>> pipelineResult = await pipelineEffect.Run(Runtime.New());

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

        Aff<Runtime, IAsyncEnumerable<Result>> next = Eff(Yield);

        Aff<Runtime, IAsyncEnumerable<Result>> pipelineEffect = pipeline.Define(request, next);

        Fin<IAsyncEnumerable<Result>> pipelineResult = await pipelineEffect.Run(Runtime.New());

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

        return;

        static async IAsyncEnumerable<Result> Yield()
        {
            await Task.Delay(1);

            yield return new Result(expResultMessage);
        }
    }
}