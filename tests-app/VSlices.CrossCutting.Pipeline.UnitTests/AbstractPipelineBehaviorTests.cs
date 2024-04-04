using FluentAssertions;
using Moq;
using System.Diagnostics;
using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.CrossCutting.Pipeline.UnitTests;

public class AbstractPipelineBehaviorTests
{
    public record RequestResult;
    public record Request : IFeature<RequestResult>;

    [Fact]
    public async Task BeforeHandleAsync_ShouldInterrumptExecution()
    {
        var pipeline = Mock.Of<AbstractPipelineBehavior<Request, RequestResult>>();
        var pipelineMock = Mock.Get(pipeline);
        var request = new Request();
        var failure = new Failure(FailureKind.ValidationError);
        var beforeHandleResult = new Result<Success>(failure);
        var expResult = new Result<RequestResult>(failure);

        RequestHandlerDelegate<RequestResult> next = () => throw new UnreachableException();

        pipelineMock.Setup(e => e.HandleAsync(request, next, default))
            .CallBase()
            .Verifiable();

        pipelineMock.Setup(e => e.BeforeHandleAsync(request, default))
            .ReturnsAsync(beforeHandleResult)
            .Verifiable();

        var result = await pipeline.HandleAsync(request, next, default);

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        result.Should().Be(expResult);

    }

    [Fact]
    public async Task InHandle_ShouldReturnFailure()
    {
        var pipeline = Mock.Of<AbstractPipelineBehavior<Request, RequestResult>>();
        var pipelineMock = Mock.Get(pipeline);
        var request = new Request();
        var result = new RequestResult();
        var failure = new Failure(FailureKind.ValidationError);

        var successResult = new Result<Success>(Success.Value);
        var expResult = new Result<RequestResult>(result);

        RequestHandlerDelegate<RequestResult> next = () => new ValueTask<Result<RequestResult>>(expResult);

        pipelineMock.Setup(e => e.HandleAsync(request, next, default))
            .CallBase()
            .Verifiable();

        pipelineMock.Setup(e => e.BeforeHandleAsync(request, default))
            .ReturnsAsync(successResult)
            .Verifiable();

        pipelineMock.Setup(e => e.InHandleAsync(request, next, default))
            .CallBase()
            .Verifiable();

        pipelineMock.Setup(e => e.AfterHandleAsync(
                request,
                It.Is<Result<RequestResult>>(e => e.Data == result),
                default
            ))
            .Verifiable();

        var handlerResult = await pipeline.HandleAsync(request, next, default);

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        handlerResult.Should().Be(expResult);

    }
}
