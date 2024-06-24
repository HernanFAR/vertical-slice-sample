using Moq;
using System.Diagnostics;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
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

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Returns(FailAff<Unit>(failure))
            .Verifiable();

        Aff<Runtime, Result> resultEffect = pipeline.Define(request, next);
        Fin<Result> result = await resultEffect.Run(Runtime.New());

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

        Aff<Runtime, Result> next = SuccessAff(expResult);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterSuccessHandling(
                request, It.Is<Result>(e => e == expResult))
            )
            .Verifiable();

        Aff<Runtime, Result> effect = pipeline.Define(request, next);
        Fin<Result> effectResult = await effect.Run(Runtime.New());

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

        Aff<Runtime, Result> next = FailAff<Runtime, Result>(failure);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandling(
                request, It.Is<Error>(e => e == failure))
            )
            .Verifiable();

        Aff<Runtime, Result> effect = pipeline.Define(request, next);
        Fin<Result> effectResult = await effect.Run(Runtime.New());

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
