using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Moq;
using System.Diagnostics;
using LanguageExt.Pipes;
using VSlices.Base.Failures;
using VSlices.Core.Stream;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.StreamPipeline.UnitTests;

public class AbstractStreamPipelineBehaviorTests
{
    public record Result;
    public record Request : IStream<Result>;

    [Fact]
    public async Task BeforeHandleAsync_ShouldInterrumptExecution()
    {
        Request request = new();
        NotFound failure = new("Testing");

        var pipeline = Mock.Of<AbstractStreamPipelineBehavior<Request, Result>>();
        Mock<AbstractStreamPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<IAsyncEnumerable<Result>> next = Eff<IAsyncEnumerable<Result>>(() => throw new UnreachableException());

        pipelineMock.Setup(e => e.BeforeHandleAsync(request, default))
            .Returns(FailAff<Unit>(failure))
            .Verifiable();

        Aff<IAsyncEnumerable<Result>> resultEffect = pipeline.Define(request, next, default);
        Fin<IAsyncEnumerable<Result>> result = await resultEffect.Run();

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

        Aff<IAsyncEnumerable<Result>> next = Eff(Yield);

        pipelineMock.Setup(e => e.BeforeHandleAsync(request, default))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandleAsync(request, next, default))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterSuccessHandlingAsync(
                request, It.IsAny<IAsyncEnumerable<Result>>(), default)
            )
            .Verifiable();

        Aff<IAsyncEnumerable<Result>> effect = pipeline.Define(request, next, default);
        Fin<IAsyncEnumerable<Result>> effectResult = await effect.Run();

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

        Aff<IAsyncEnumerable<Result>> next = FailAff<IAsyncEnumerable<Result>>(failure);

        pipelineMock.Setup(e => e.BeforeHandleAsync(request, default))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandleAsync(request, next, default))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandlingAsync(
                request, It.Is<Error>(e => e == failure), default)
            )
            .Verifiable();

        Aff<IAsyncEnumerable<Result>> effect = pipeline.Define(request, next, default);
        Fin<IAsyncEnumerable<Result>> effectResult = await effect.Run();

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
