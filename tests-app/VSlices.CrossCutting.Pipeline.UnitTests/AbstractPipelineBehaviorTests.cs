﻿using Moq;
using System.Diagnostics;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Core;
using VSlices.Core.Traits;

namespace VSlices.CrossCutting.Pipeline.UnitTests;

public class AbstractPipelineBehaviorTests
{
    public record Result;
    public record Request : IFeature<Result>;

    [Fact]
    public Task BeforeHandleAsync_ShouldInterruptExecution()
    {
        Request request = new();
        NotFound failure = new("Testing");

        var pipeline = Mock.Of<AbstractPipelineBehavior<Request, Result>>();
        Mock<AbstractPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        Eff<Result> next = liftEff<Result>(async () => throw new UnreachableException());
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        pipelineMock.Setup(e => e.BeforeHandle(request))
                    .Returns(FailEff<Unit>(failure))
                    .Verifiable();

        Eff<HandlerRuntime, Result> effect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var runtime = HandlerRuntime.New(dependencyProvider);

        Fin<Result> result = effect.Run(runtime, default(CancellationToken));

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = result.Match(
            _  => throw new UnreachableException(),
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

        var pipeline = Mock.Of<AbstractPipelineBehavior<Request, Result>>();
        Mock<AbstractPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<HandlerRuntime, Result> next = SuccessEff(expResult);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterSuccessHandling(
                request, It.Is<Result>(e => e == expResult))
            )
            .Verifiable();

        Eff<HandlerRuntime, Result> effect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var runtime = HandlerRuntime.New(dependencyProvider);

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
        NotFound failure = new("Testing");

        var pipeline = Mock.Of<AbstractPipelineBehavior<Request, Result>>();
        Mock<AbstractPipelineBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<HandlerRuntime, Result> next = FailEff<HandlerRuntime, Result>(failure);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandling(
                request, It.Is<Error>(e => e == failure))
            )
            .Verifiable();

        Eff<HandlerRuntime, Result> effect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var runtime = HandlerRuntime.New(dependencyProvider);

        var         result_      = liftEff<HandlerRuntime, Result>(_ => failure).Run(runtime, default(CancellationToken));

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
