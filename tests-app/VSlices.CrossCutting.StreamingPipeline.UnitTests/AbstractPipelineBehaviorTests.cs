using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Moq;
using System.Diagnostics;
using LanguageExt.Pipes;
using LanguageExt.SysX.Live;
using VSlices.Base.Failures;
using VSlices.Core.Stream;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.StreamPipeline.UnitTests;

public class AbstractStreamPipelineBehaviorTests
{
    public record Result;
    public record Request : IStream<Result>;

    [Fact]
    public async Task BeforeHandleAsync_ShouldInterruptExecution()
    {
        Request request = new();
        NotFound failure = new("Testing");

        var pipeline = Mock.Of<AbstractStreamPipelineBehavior<Request, Result>>();
        Mock<AbstractStreamPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<IAsyncEnumerable<Result>> next = Eff<IAsyncEnumerable<Result>>(() => throw new UnreachableException());

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Returns(FailAff<Unit>(failure))
            .Verifiable();

        Aff<Runtime, IAsyncEnumerable<Result>> resultEffect = pipeline.Define(request, next);
        Fin<IAsyncEnumerable<Result>> result = await resultEffect.Run(Runtime.New());

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = result.Match(
            Succ: _ => throw new UnreachableException(),
            Fail: error =>
            {
                error.Should().Be(failure);

                return unit;
            });

    }

    [Fact]
    public async Task InHandle_ShouldReturnResult()
    {
        Request request = new();
        Result expResult = new();

        AbstractStreamPipelineBehavior<Request, Result> pipeline = Mock.Of<AbstractStreamPipelineBehavior<Request, Result>>();
        Mock<AbstractStreamPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Aff<Runtime, IAsyncEnumerable<Result>> next = Eff(Yield);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterSuccessHandling(
                request, It.IsAny<IAsyncEnumerable<Result>>())
            )
            .Verifiable();

        Aff<Runtime, IAsyncEnumerable<Result>> effect = pipeline.Define(request, next);
        Fin<IAsyncEnumerable<Result>> effectResult = await effect.Run(Runtime.New());

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = await effectResult.Match(
            Succ: async enumerable =>
            {
                await foreach (Result result in enumerable)
                {
                    result.Should().Be(expResult);
                }

                return unit;
            },
            _ => throw new UnreachableException());
        return;

        async IAsyncEnumerable<Result> Yield()
        {
            await Task.Delay(1);

            yield return expResult;
        }
    }

    [Fact]
    public async Task InHandle_ShouldReturnFailure()
    {
        Request request = new();
        NotFound failure = new("Testing");

        var pipeline = Mock.Of<AbstractStreamPipelineBehavior<Request, Result>>();
        Mock<AbstractStreamPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Aff<Runtime, IAsyncEnumerable<Result>> next = FailAff<Runtime, IAsyncEnumerable<Result>>(failure);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandling(
                request, It.Is<Error>(e => e == failure))
            )
            .Verifiable();

        Aff<Runtime, IAsyncEnumerable<Result>> effect = pipeline.Define(request, next);
        Fin<IAsyncEnumerable<Result>> effectResult = await effect.Run(Runtime.New());

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = effectResult.Match(
            _ => throw new UnreachableException(),
            success =>
            {
                success.Should().Be(failure);

                return unit;
            });

    }
}
