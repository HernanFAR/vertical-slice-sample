using System.Diagnostics;
using FluentAssertions;
using Moq;
using VSlices.Base;
using LanguageExt;
using LanguageExt.Common;
using VSlices.Base.Failures;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling.UnitTests;

public class AbstractExceptionHandlingBehaviorTests
{
    public record Result;
    public record Request : IFeature<Result>;

    [Fact]
    public async Task InHandle_ShouldReturnSuccess()
    {
        Request request = new();
        Result result = new();

        var pipeline = Mock.Of<AbstractPipelineBehavior<Request, Result>>();
        Mock<AbstractPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;
        
        Aff<Result> next = SuccessAff(result);

        pipelineMock.Setup(e => e.BeforeHandleAsync(request, default))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandleAsync(request, next, default))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterSuccessHandlingAsync(
                request, It.Is<Result>(e => e == result), default))
            .Verifiable();

        Aff<Result> pipelineEffect = pipeline.Define(request, next, default);
        Fin<Result> pipelineResult = await pipelineEffect.Run();

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = pipelineResult.Match(
            r => r.Should().Be(r),
            _ => throw new UnreachableException());

    }

    [Fact]
    public async Task InHandle_ShouldReturnFailure()
    {
        Request request = new();
        Exception expEx = new();
        
        var pipeline = Mock.Of<AbstractExceptionHandlingBehavior<Request, Result>>();
        Mock<AbstractExceptionHandlingBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Aff<Result> next = Aff<Result>(() => throw expEx);

        pipelineMock.Setup(e => e.BeforeHandleAsync(request, default))
            .Verifiable();

        pipelineMock.Setup(e => e.ProcessExceptionAsync(expEx, request, default))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandleAsync(request, next, default))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandlingAsync(
                request, It.Is<ServerError>(e => e.Message == "Internal server error"), default))
            .Verifiable();

        Aff<Result> pipelineEffect = pipeline.Define(request, next, default);
        Fin<Result> pipelineResult = await pipelineEffect.Run();

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = pipelineResult.Match(
            _ => throw new UnreachableException(),
            error =>
            {
                error.Should().BeOfType<ServerError>();
                error.Message.Should().Be("Internal server error");

                return unit;
            });

    }
}