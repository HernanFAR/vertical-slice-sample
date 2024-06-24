using System.Diagnostics;
using FluentAssertions;
using Moq;
using VSlices.Base;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
using VSlices.Base.Failures;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling.UnitTests;

public class AbstractExceptionHandlingBehaviorTests
{
    public record Result;
    public record Request : IFeature<Result>;

    [Fact]
    public async Task InHandle_ShouldReturnFailure()
    {
        Request request = new();
        Exception expEx = new();
        
        var pipeline = Mock.Of<AbstractExceptionHandlingBehavior<Request, Result>>();
        Mock<AbstractExceptionHandlingBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Aff<Runtime, Result> next = Aff<Runtime, Result>(_ => throw expEx);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.Process(expEx, request))
            .Returns(AffMaybe<Runtime, Result>(_ => ValueTask.FromResult<Fin<Result>>(new ServerError("Internal server error").AsError())))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandling(
                request, It.Is<ServerError>(e => e.Message == "Internal server error")))
            .Verifiable();

        Aff<Runtime, Result> pipelineEffect = pipeline.Define(request, next);
        Fin<Result> pipelineResult = await pipelineEffect.Run(Runtime.New());

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