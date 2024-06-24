using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt.SysX.Live;
using VSlices.CrossCutting.Pipeline;
using static LanguageExt.Prelude;

namespace VSlices.Core.UseCases.Reflection.UnitTests;

public class ReflectionRequestRunnerTests
{
    public sealed class Accumulator
    {
        public int Count { get; set; }

        public string Str { get; set; } = "";

    }

    public class PipelineBehaviorOne<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        readonly Accumulator _accumulator;

        public PipelineBehaviorOne(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<Runtime, TResponse> Define(TRequest request, Aff<Runtime, TResponse> next) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "OpenPipelineOne_";

                return unit;
            })
            from result in next
            select result;
    }

    public class PipelineBehaviorTwo<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        readonly Accumulator _accumulator;

        public PipelineBehaviorTwo(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<Runtime, TResponse> Define(TRequest request, Aff<Runtime, TResponse> next) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "OpenPipelineTwo_";

                return unit;
            })
            from result in next
            select result;
    }

    public class ConcretePipelineBehaviorOne : IPipelineBehavior<RequestOne, Unit>
    {
        readonly Accumulator _accumulator;

        public ConcretePipelineBehaviorOne(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<Runtime, Unit> Define(RequestOne request, Aff<Runtime, Unit> next) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "ConcretePipelineOne_";

                return unit;
            })
            from result in next
            select result;
    }

    public record RequestOne : IRequest;

    public class HandlerOne : IHandler<RequestOne, Unit>
    {
        readonly Accumulator _accumulator;

        public HandlerOne(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<Runtime, Unit> Define(RequestOne requestOne) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "HandlerOne_";

                return unit;
            })
            select unit;
    }

    public record RequestTwo : IRequest<Unit>;

    public class HandlerTwo : IHandler<RequestTwo, Unit>
    {
        readonly Accumulator _accumulator;

        public HandlerTwo(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<Runtime, Unit> Define(RequestTwo request) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "EventHandlerTwo_";

                return unit;
            })
            select unit;
    }

    [Fact]
    public async Task Sender_Should_CallHandler()
    {
        const int expCount = 1;
        ServiceCollection services = new();

        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddTransient<IRequestRunner, ReflectionRequestRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = await sender.RunAsync(new RequestOne(), default(CancellationToken));

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Str.Should().Be("HandlerOne_");
        accumulator.Count.Should().Be(expCount);

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndOpenPipeline()
    {
        const int expCount = 2;
        ServiceCollection services = new();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddTransient<IRequestRunner, ReflectionRequestRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = await sender.RunAsync(new RequestOne(), default(CancellationToken));

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_HandlerOne_");

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndOpenPipelineAndClosedPipeline()
    {
        const int expCount = 3;
        ServiceCollection services = new();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddTransient<IRequestRunner, ReflectionRequestRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = await sender.RunAsync(new RequestOne(), default(CancellationToken));

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_ConcretePipelineOne_HandlerOne_");
    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndTwoOpenPipeline()
    {
        const int expCount = 3;
        ServiceCollection services = new();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddTransient<IRequestRunner, ReflectionRequestRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = await sender.RunAsync(new RequestOne(), default(CancellationToken));

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_HandlerOne_");

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndTwoOpenPipelineAndOneClosedPipeline()
    {
        const int expCount = 4;
        ServiceCollection services = new();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddTransient<IRequestRunner, ReflectionRequestRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = await sender.RunAsync(new RequestOne(), default(CancellationToken));

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_ConcretePipelineOne_HandlerOne_");

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndTwoOpenPipelineAndNoneClosedPipeline()
    {
        const int expCount = 3;
        ServiceCollection services = new();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddTransient<IHandler<RequestTwo, Unit>, HandlerTwo>();
        services.AddTransient<IRequestRunner, ReflectionRequestRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = await sender.RunAsync(new RequestTwo(), default(CancellationToken));

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerTwo_");

    }
}