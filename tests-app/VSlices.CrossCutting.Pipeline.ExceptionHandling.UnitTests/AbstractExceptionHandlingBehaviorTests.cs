using System.Diagnostics;
using FluentAssertions;
using Moq;
using VSlices.Base;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Failures;
using VSlices.Core;
using VSlices.Core.Traits;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling.UnitTests;

public class AbstractExceptionHandlingBehaviorTests
{
    public record Result;
    public record Request : IFeature<Result>;

    [Fact]
    public void InHandle_ShouldReturnFailure()
    {
        Request request = new();
        Exception expEx = new();
        
        var pipeline = Mock.Of<AbstractExceptionHandlingBehavior<Request, Result>>();
        Mock<AbstractExceptionHandlingBehavior<Request, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        Eff<HandlerRuntime, Result> next = liftEff<HandlerRuntime, Result>(async _ => throw expEx);

        pipelineMock.Setup(e => e.BeforeHandle(request))
            .Verifiable();

        pipelineMock.Setup(e => e.Process(expEx, request))
            .Returns(liftEff<HandlerRuntime, Result>(_ => new ServerError("Internal server error").AsError()))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(request, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandling(
                request, It.Is<Error>(e => e.Message == "Internal server error")))
            .Verifiable();

        Eff<HandlerRuntime, Result> pipelineEffect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var runtime = HandlerRuntime.New(dependencyProvider, EnvIO.New());

        Fin<Result> effectResult = pipelineEffect.Run(runtime, runtime.EnvIO);

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = effectResult.Match(_ => throw new UnreachableException(),
                               error =>
                               {
                                   error.Should().BeOfType<ServerError>();
                                   error.Message.Should().Be("Internal server error");

                                   return unit;
                               });

    }
}