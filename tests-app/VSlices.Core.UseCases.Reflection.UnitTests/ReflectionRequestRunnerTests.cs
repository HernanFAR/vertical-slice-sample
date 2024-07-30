using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using VSlices.Base;
using VSlices.CrossCutting.Pipeline;
using static LanguageExt.Prelude;
using static VSlices.VSlicesPrelude;

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
        public Eff<VSlicesRuntime, TResponse> Define(TRequest request, Eff<VSlicesRuntime, TResponse> next) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() =>
            {
                accumulator.Count += 1;
                accumulator.Str += "OpenPipelineOne_";

                return unit;
            })
            from result in next
            select result;
    }

    public class PipelineBehaviorTwo<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Eff<VSlicesRuntime, TResponse> Define(TRequest request, Eff<VSlicesRuntime, TResponse> next) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() =>
            {
                accumulator.Count += 1;
                accumulator.Str += "OpenPipelineTwo_";

                return unit;
            })
            from result in next
            select result;
    }

    public class ConcretePipelineBehaviorOne : IPipelineBehavior<RequestOne, Unit>
    {
        public Eff<VSlicesRuntime, Unit> Define(RequestOne request, Eff<VSlicesRuntime, Unit> next) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() =>
            {
                accumulator.Count += 1;
                accumulator.Str += "ConcretePipelineOne_";

                return unit;
            })
            from result in next
            select result;
    }

    public record RequestOne : IRequest;

    public class HandlerOne : IHandler<RequestOne, Unit>
    {        
        public Eff<VSlicesRuntime, Unit> Define(RequestOne requestOne) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() =>
            {
                accumulator.Count += 1;
                accumulator.Str += "HandlerOne_";

                return unit;
            })
            select unit;
    }

    public record RequestTwo : IRequest<Unit>;

    public class HandlerTwo : IHandler<RequestTwo, Unit>
    {
        public Eff<VSlicesRuntime, Unit> Define(RequestTwo request) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() =>
            {
                accumulator.Count += 1;
                accumulator.Str += "EventHandlerTwo_";

                return unit;
            })
            select unit;
    }

    [Fact]
    public Task Sender_Should_CallHandler()
    {
        const int expCount = 1;
        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient<IHandler<RequestOne, Unit>, HandlerOne>()
                       .AddTransient<IRequestRunner, ReflectionRequestRunner>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new RequestOne());

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Str.Should().Be("HandlerOne_");
        accumulator.Count.Should().Be(expCount);
        return Task.CompletedTask;
    }

    [Fact]
    public Task Sender_Should_CallHandlerAndOpenPipeline()
    {
        const int expCount = 2;
        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>))
                       .AddTransient<IHandler<RequestOne, Unit>, HandlerOne>()
                       .AddTransient<IRequestRunner, ReflectionRequestRunner>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new RequestOne());

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_HandlerOne_");
        return Task.CompletedTask;
    }

    [Fact]
    public Task Sender_Should_CallHandlerAndOpenPipelineAndClosedPipeline()
    {
        const int expCount = 3;
        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>))
                       .AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne))
                       .AddTransient<IHandler<RequestOne, Unit>, HandlerOne>()
                       .AddTransient<IRequestRunner, ReflectionRequestRunner>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new RequestOne());

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_ConcretePipelineOne_HandlerOne_");
        return Task.CompletedTask;
    }

    [Fact]
    public Task Sender_Should_CallHandlerAndTwoOpenPipeline()
    {
        const int expCount = 3;
        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>))
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>))
                       .AddTransient<IHandler<RequestOne, Unit>, HandlerOne>()
                       .AddTransient<IRequestRunner, ReflectionRequestRunner>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new RequestOne());

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_HandlerOne_");
        return Task.CompletedTask;
    }

    [Fact]
    public Task Sender_Should_CallHandlerAndTwoOpenPipelineAndOneClosedPipeline()
    {
        const int expCount = 4;
        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>))
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>))
                       .AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne))
                       .AddTransient<IHandler<RequestOne, Unit>, HandlerOne>()
                       .AddTransient<IRequestRunner, ReflectionRequestRunner>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new RequestOne());
        
        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_ConcretePipelineOne_HandlerOne_");
        return Task.CompletedTask;
    }

    [Fact]
    public Task Sender_Should_CallHandlerAndTwoOpenPipelineAndNoneClosedPipeline()
    {
        const int expCount = 3;
        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>))
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>))
                       .AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne))
                       .AddTransient<IHandler<RequestOne, Unit>, HandlerOne>()
                       .AddTransient<IHandler<RequestTwo, Unit>, HandlerTwo>()
                       .AddTransient<IRequestRunner, ReflectionRequestRunner>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new RequestTwo());

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerTwo_");
        return Task.CompletedTask;
    }
}