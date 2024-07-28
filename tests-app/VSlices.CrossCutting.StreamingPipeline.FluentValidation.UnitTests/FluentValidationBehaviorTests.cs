using FluentAssertions;
using FluentValidation;
using System.Diagnostics;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Core;
using VSlices.Core.Stream;
using VSlices.Core.Traits;

namespace VSlices.CrossCutting.StreamPipeline.FluentValidation.UnitTests;

public class AbstractExceptionHandlingBehaviorTests
{
    public record Result(string Value);

    public record Request(string Value) : IStream<Result>;

    public class Validator : AbstractValidator<Request>
    {
        public const string ValueEmptyMessage = "Test message";

        public Validator()
        {
            RuleFor(e => e.Value).NotEmpty().WithMessage(ValueEmptyMessage);
        }
    }

    [Fact]
    public async Task BeforeHandleAsync_ShouldInterruptExecution()
    {
        const int expErrorCount = 1;
        FluentValidationStreamBehavior<Request, Result> pipeline = new();
        Request request = new(null!);

        Eff<HandlerRuntime, IAsyncEnumerable<Result>> next =
            lift<HandlerRuntime,
                 IAsyncEnumerable<Result>>(_ => throw new UnreachableException());

        Eff<HandlerRuntime, IAsyncEnumerable<Result>> pipelineEffect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection()
            .AddTransient<IValidator<Request>, Validator>()
            .BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var runtime = HandlerRuntime.New(dependencyProvider, EnvIO.New());

        Fin<IAsyncEnumerable<Result>> result = pipelineEffect.Run(runtime);

        result
            .Match(Succ: _ => throw new UnreachableException(),
                   Fail: failure =>
                   {
                       var unprocessable = (Unprocessable)failure;

                       unprocessable.Errors.Should()
                                    .HaveCount(expErrorCount);
                       unprocessable.Errors[0]
                                    .Name.Should()
                                    .Be(nameof(Request.Value));
                       unprocessable.Errors[0]
                                    .Detail.Should()
                                    .Be(Validator.ValueEmptyMessage);

                       return unit;
                   });
    }

    [Fact]
    public async Task BeforeHandleAsync_ShouldProcess()
    {
        const string expResultMessage = "testing :D";
        FluentValidationStreamBehavior<Request, Result> pipeline = new();
        Request request = new(expResultMessage);

        Eff<HandlerRuntime, IAsyncEnumerable<Result>> next = liftEff(Yield);

        Eff<HandlerRuntime, IAsyncEnumerable<Result>> pipelineEffect = pipeline.Define(request, next);

        ServiceProvider provider = new ServiceCollection()
            .AddTransient<IValidator<Request>, Validator>()
            .BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var runtime = HandlerRuntime.New(dependencyProvider, EnvIO.New());
        
        Fin<IAsyncEnumerable<Result>> pipelineResult = pipelineEffect.Run(runtime);

        await pipelineResult
            .Match(async enumeration =>
                   {
                       await foreach (Result val in enumeration)
                       {
                           val.Value.Should()
                              .Be(expResultMessage);
                       }
                   },
                   _ => throw new UnreachableException());

        return;

        static async IAsyncEnumerable<Result> Yield()
        {
            await Task.Delay(1);

            yield return new Result(expResultMessage);
        }
    }
}