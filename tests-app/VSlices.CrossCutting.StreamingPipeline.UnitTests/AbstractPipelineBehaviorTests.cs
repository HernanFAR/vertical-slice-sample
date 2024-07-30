using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Moq;
using System.Diagnostics;
using LanguageExt.Pipes;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Failures;
using VSlices.Core;
using VSlices.Core.Stream;
using VSlices.Core.Traits;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.StreamPipeline.UnitTests;

public class AbstractStreamPipelineBehaviorTests
{
    public record Result;
    public record Request : IStream<Result>;

    [Fact]
    public Task BeforeHandleAsync_ShouldInterruptExecution()
    {
        Request request = new();
        NotFound failure = new("Testing");

        var pipeline = Mock.Of<AbstractStreamPipelineBehavior<Request, Result>>();
        Mock<AbstractStreamPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<IAsyncEnumerable<Result>> next = liftEff<IAsyncEnumerable<Result>>(() => Error.New(new UnreachableException()));

        pipelineMock.Setup(e => e.BeforeHandle(request))
                    .Returns(FailEff<Unit>(failure))
                    .Verifiable();

        Eff<HandlerRuntime, IAsyncEnumerable<Result>> effect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var                runtime            = HandlerRuntime.New(dependencyProvider);

        Fin<IAsyncEnumerable<Result>> effectResult = effect.Run(runtime, default(CancellationToken));

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = effectResult.Match(Succ: _ => throw new UnreachableException(),
                               Fail: error =>
                               {
                                   error.Should().Be(failure);

                                   return unit;
                               });
        return Task.CompletedTask;
    }

    [Fact]
    public async Task InHandle_ShouldReturnResult()
    {
        Request request = new();
        Result expResult = new();

        AbstractStreamPipelineBehavior<Request, Result> pipeline = Mock.Of<AbstractStreamPipelineBehavior<Request, Result>>();
        Mock<AbstractStreamPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<HandlerRuntime, IAsyncEnumerable<Result>> next = liftEff(Yield);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterSuccessHandling(
                request, It.IsAny<IAsyncEnumerable<Result>>())
            )
            .Verifiable();

        Eff<HandlerRuntime, IAsyncEnumerable<Result>> effect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var runtime = HandlerRuntime.New(dependencyProvider);

        Fin<IAsyncEnumerable<Result>> effectResult = effect.Run(runtime, default(CancellationToken));

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
    public Task InHandle_ShouldReturnFailure()
    {
        Request request = new();
        NotFound failure = new("Testing");

        var pipeline = Mock.Of<AbstractStreamPipelineBehavior<Request, Result>>();
        Mock<AbstractStreamPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<HandlerRuntime, IAsyncEnumerable<Result>> next = FailEff<HandlerRuntime, IAsyncEnumerable<Result>>(failure);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandling(
                request, It.Is<Error>(e => e == failure))
            )
            .Verifiable();

        Eff<HandlerRuntime, IAsyncEnumerable<Result>> effect = pipeline.Define(request, next);


        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var runtime = HandlerRuntime.New(dependencyProvider);

        Fin<IAsyncEnumerable<Result>> effectResult = effect.Run(runtime, default(CancellationToken));

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = effectResult.Match(
            _ => throw new UnreachableException(),
            success =>
            {
                success.Should().Be(failure);

                return unit;
            });
        return Task.CompletedTask;
    }
}
