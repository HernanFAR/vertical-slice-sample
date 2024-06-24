using System.Diagnostics;
using FluentAssertions;
using Moq;
using VSlices.Base;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
using VSlices.Base.Failures;
using VSlices.Core.Stream;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.StreamPipeline.ExceptionHandling.UnitTests;

public class AbstractExceptionHandlingStreamBehaviorTests
{
    public record Result;
    public record Request : IStream<Result>;

    
    [Fact]
    public async Task InHandle_ShouldReturnFailure()
    {
        Request request = new();
        Exception expEx = new();
        
        var pipeline = Mock.Of<AbstractExceptionHandlingStreamBehavior<Request, Result>>();
        Mock<AbstractExceptionHandlingStreamBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Aff<Runtime, IAsyncEnumerable<Result>> next = Aff<Runtime, IAsyncEnumerable<Result>>((env) => throw expEx);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.Process(expEx, request))
            .Returns(AffMaybe<Runtime, IAsyncEnumerable<Result>>(_ => ValueTask.FromResult<Fin<IAsyncEnumerable<Result>>>(new ServerError("Internal server error").AsError())))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandling(
                request, It.Is<ServerError>(e => e.Message == "Internal server error")))
            .Verifiable();

        Aff<Runtime, IAsyncEnumerable<Result>> pipelineEffect = pipeline.Define(request, next);
        Fin<IAsyncEnumerable<Result>> pipelineResult = await pipelineEffect.Run(Runtime.New());

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
