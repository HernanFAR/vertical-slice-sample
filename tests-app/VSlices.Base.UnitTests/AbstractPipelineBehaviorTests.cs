using System.Diagnostics;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Base.Failures;
using VSlices.Base.Traits;
using static LanguageExt.Prelude;

namespace VSlices.Base.UnitTests;

public class AbstractPipelineBehaviorTests
{
    public record Result;
    public record Request : IFeature<Result>;

    [Fact]
    public Task BeforeHandleAsync_ShouldInterruptExecution()
    {
        Request request = new();
        ExtensibleExpected failure = ExtensibleExpected.NotFound("Testing", []);

        AbstractPipelineBehavior<Request, Result> pipeline = Mock.Of<AbstractPipelineBehavior<Request, Result>>();
        Mock<AbstractPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        Eff<Result> next = liftEff<Result>(async () => throw new UnreachableException());
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        pipelineMock.Setup(e => e.BeforeHandle(request))
                    .Returns(FailEff<Unit>(failure))
                    .Verifiable();

        Eff<VSlicesRuntime, Result> effect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        VSlicesRuntime runtime = VSlicesRuntime.New(dependencyProvider);

        Fin<Result> result = effect.Run(runtime, default(CancellationToken));

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = result.Match(
            _ => throw new UnreachableException(),
            error =>
            {
                error.Should().Be(failure);

                return unit;
            });
        return Task.CompletedTask;
    }

    [Fact]
    public Task InHandle_ShouldReturnResult()
    {
        Request request = new();
        Result expResult = new();

        AbstractPipelineBehavior<Request, Result> pipeline = Mock.Of<AbstractPipelineBehavior<Request, Result>>();
        Mock<AbstractPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<VSlicesRuntime, Result> next = SuccessEff(expResult);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterSuccessHandling(
                request, It.Is<Result>(e => e == expResult))
            )
            .Verifiable();

        Eff<VSlicesRuntime, Result> effect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        VSlicesRuntime runtime = VSlicesRuntime.New(dependencyProvider);

        Fin<Result> effectResult = effect.Run(runtime, default(CancellationToken));

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = effectResult.Match(
            success =>
            {
                success.Should().Be(expResult);

                return unit;
            },
            _ => throw new UnreachableException());
        return Task.CompletedTask;
    }

    [Fact]
    public Task InHandle_ShouldReturnFailure()
    {
        Request request = new();
        ExtensibleExpected failure = ExtensibleExpected.NotFound("Testing", []);

        AbstractPipelineBehavior<Request, Result> pipeline = Mock.Of<AbstractPipelineBehavior<Request, Result>>();
        Mock<AbstractPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<VSlicesRuntime, Result> next = FailEff<VSlicesRuntime, Result>(failure);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandling(
                request, It.Is<Error>(e => e == failure))
            )
            .Verifiable();

        Eff<VSlicesRuntime, Result> effect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        VSlicesRuntime runtime = VSlicesRuntime.New(dependencyProvider);

        Fin<Result> effectResult = effect.Run(runtime, default(CancellationToken));

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
