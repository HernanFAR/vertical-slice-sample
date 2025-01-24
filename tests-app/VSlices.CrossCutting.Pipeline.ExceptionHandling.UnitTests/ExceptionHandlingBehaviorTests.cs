using System.Diagnostics;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Base.Traits;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Interceptor.ExceptionHandling.UnitTests;

public class ExceptionHandlingInterceptorTests
{
    public record Result;
    public record Input;

    [Fact]
    public void InHandle_ShouldReturnFailure()
    {
        Input input = new();
        Exception expEx = new();
        
        var pipeline = Mock.Of<ExceptionHandlingInterceptor<Input, Result>>();
        Mock<ExceptionHandlingInterceptor<Input, Result>> pipelineMock = Mock.Get(pipeline);
        pipelineMock.CallBase = true;

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        Eff<VSlicesRuntime, Result> next = liftEff<VSlicesRuntime, Result>(async _ => throw expEx);
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        pipelineMock.Setup(e => e.BeforeHandle(input))
                    .Verifiable();

        pipelineMock.Setup(e => e.Process(expEx, input))
            .Returns(liftEff<VSlicesRuntime, Result>(_ =>VSlicesPrelude.serverError("Internal server error")))
            .Verifiable();

        pipelineMock.Setup(e => e.InHandle(input, next))
            .Verifiable();

        pipelineMock.Setup(e => e.AfterFailureHandling(
                input, It.Is<Error>(e => e.Message == "Internal server error")))
            .Verifiable();

        Eff<VSlicesRuntime, Result> pipelineEffect = pipeline.Define(input, next);

        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var runtime = VSlicesRuntime.New(dependencyProvider);

        Fin<Result> effectResult = pipelineEffect.Run(runtime, default(CancellationToken));

        pipelineMock.Verify();
        pipelineMock.VerifyNoOtherCalls();

        _ = effectResult.Match(_ => throw new UnreachableException(),
                               error =>
                               {
                                   error.Code.Should().Be(500);
                                   error.Message.Should().Be("Internal server error");
                                   error.Should().BeOfType<ExtensibleExpected>();

                                   return unit;
                               });

    }
}