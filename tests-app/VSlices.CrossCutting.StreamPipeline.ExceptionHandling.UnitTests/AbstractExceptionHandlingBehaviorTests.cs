using System.Diagnostics;
using FluentAssertions;
using Moq;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Core;
using VSlices.Core.Stream;
using VSlices.Base.Traits;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.StreamPipeline.ExceptionHandling.UnitTests;

public class AbstractExceptionHandlingStreamBehaviorTests
{
    public record Result;
    public record Request : IStream<Result>;

    
    [Fact]
    public Task InHandle_ShouldReturnFailure()
    {
        Request request = new();
        Exception expEx = new();
        
        var pipeline = Mock.Of<AbstractExceptionHandlingStreamBehavior<Request, Result>>();
        Mock<AbstractExceptionHandlingStreamBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<VSlicesRuntime, IAsyncEnumerable<Result>> next = liftEff<VSlicesRuntime, IAsyncEnumerable<Result>>(env => Error.New(expEx));

        pipelineMock.Setup(e => e.BeforeHandle(request))
                    .Verifiable();

        pipelineMock.Setup(e => e.Process(expEx, request))
            .Returns(liftEff<VSlicesRuntime, IAsyncEnumerable<Result>>(_ => new ServerError("Internal server error").AsError()))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandling(
                request, It.Is<ServerError>(e => e.Message == "Internal server error")))
            .Verifiable();

        Eff<VSlicesRuntime, IAsyncEnumerable<Result>> pipelineEffect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var runtime = VSlicesRuntime.New(dependencyProvider);

        Fin<IAsyncEnumerable<Result>> pipelineResult = pipelineEffect.Run(runtime, default(CancellationToken));

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
        return Task.CompletedTask;
    }

}
