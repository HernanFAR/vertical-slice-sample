using Moq;
using System.Diagnostics;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Base.Failures;

namespace VSlices.CrossCutting.Pipeline.UnitTests;

public class AbstractPipelineBehaviorTests
{
    public record Result;
    public record Request : IFeature<Result>;

    [Fact]
    public async Task BeforeHandleAsync_ShouldInterrumptExecution()
    {
        Request request = new();
        NotFound failure = new("Testing");

        var pipeline = Mock.Of<AbstractPipelineBehavior<Request, Result>>();
        Mock<AbstractPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<Result> next = Eff<Result>(() => throw new UnreachableException());

        pipelineMock.Setup(e => e.BeforeHandleAsync(request, default))
            .Returns(FailAff<Unit>(failure))
            .Verifiable();

        Aff<Result> resultEffect = pipeline.Define(request, next, default);
        Fin<Result> result = await resultEffect.Run();

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = result.Match(
            _  => throw new UnreachableException(),
            error =>
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

        var pipeline = Mock.Of<AbstractPipelineBehavior<Request, Result>>();
        Mock<AbstractPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Aff<Result> next = SuccessAff(expResult);

        pipelineMock.Setup(e => e.BeforeHandleAsync(request, default))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandleAsync(request, next, default))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterSuccessHandlingAsync(
                request, It.Is<Result>(e => e == expResult), default)
            )
            .Verifiable();

        Aff<Result> effect = pipeline.Define(request, next, default);
        Fin<Result> effectResult = await effect.Run();

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = effectResult.Match(
            success =>
            {
                success.Should().Be(expResult);

                return unit;
            },
            _ => throw new UnreachableException());

    }

    [Fact]
    public async Task InHandle_ShouldReturnFailure()
    {
        Request request = new();
        NotFound failure = new("Testing");

        var pipeline = Mock.Of<AbstractPipelineBehavior<Request, Result>>();
        Mock<AbstractPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Aff<Result> next = FailAff<Result>(failure);

        pipelineMock.Setup(e => e.BeforeHandleAsync(request, default))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandleAsync(request, next, default))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandlingAsync(
                request, It.Is<Error>(e => e == failure), default)
            )
            .Verifiable();

        Aff<Result> effect = pipeline.Define(request, next, default);
        Fin<Result> effectResult = await effect.Run();

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
