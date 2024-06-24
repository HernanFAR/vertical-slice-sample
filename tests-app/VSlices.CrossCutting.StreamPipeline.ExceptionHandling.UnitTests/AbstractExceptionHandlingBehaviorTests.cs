using System.Diagnostics;
using FluentAssertions;
using Moq;
using VSlices.Base;
using LanguageExt;
using LanguageExt.Common;
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

        Aff<IAsyncEnumerable<Result>> next = Aff<IAsyncEnumerable<Result>>(() => throw expEx);

        pipelineMock.Setup(e => e.BeforeHandleAsync(request, default))
            .Verifiable();

        pipelineMock.Setup(e => e.ProcessExceptionAsync(expEx, request, default))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandleAsync(request, next, default))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandlingAsync(
                request, It.Is<ServerError>(e => e.Message == "Internal server error"), default))
            .Verifiable();

        Aff<IAsyncEnumerable<Result>> pipelineEffect = pipeline.Define(request, next, default);
        Fin<IAsyncEnumerable<Result>> pipelineResult = await pipelineEffect.Run();

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
