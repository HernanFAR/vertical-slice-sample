using FluentAssertions;
using FluentValidation;
using System.Diagnostics;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Base.Traits;
using VSlices.Core;

namespace VSlices.CrossCutting.Pipeline.FluentValidation.UnitTests;

public class AbstractExceptionHandlingBehaviorTests
{
    public record RequestResult;

    public record Request(string Value) : IFeature<RequestResult>;

    public class Validator : AbstractValidator<Request>
    {
        public const string ValueEmptyMessage = "Test message";

        public Validator()
        {
            RuleFor(e => e.Value).NotEmpty().WithMessage(ValueEmptyMessage);
        }
    }

    [Fact]
    public Task BeforeHandleAsync_ShouldInterruptExecution()
    {
        const int expErrorCount = 1;
        FluentValidationBehavior<Request, RequestResult> pipeline = new();
        Request request = new(null!);

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        Eff<VSlicesRuntime, RequestResult> next = liftEff<VSlicesRuntime, RequestResult>(async _ => throw new UnreachableException());
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        Eff<VSlicesRuntime, RequestResult> pipelineEffect = pipeline.Define(request, next);


        ServiceProvider provider = new ServiceCollection()
            .AddTransient<IValidator<Request>, Validator>()
            .BuildServiceProvider();

        DependencyProvider dependencyProvider = new(provider);
        var runtime = VSlicesRuntime.New(dependencyProvider);

        Fin<RequestResult> pipelineResult = pipelineEffect.Run(runtime, EnvIO.New());

        _ = pipelineResult
            .Match(
                _ => throw new UnreachableException(),
                failure =>
                {
                    var unprocessable = failure.Should().BeOfType<ExtensibleExpected>();

                    unprocessable.Subject.Extensions["errors"]
                                 .Should()
                                 .BeOfType<Dictionary<string, string[]>>()
                                 .Subject.Should()
                                 .HaveCount(expErrorCount)
                                 .And
                                 .Subject[nameof(Request.Value)]
                                 .Should()
                                 .BeEquivalentTo(new[] { Validator.ValueEmptyMessage });

                    return unit;
                }
            );
        return Task.CompletedTask;
    }
}