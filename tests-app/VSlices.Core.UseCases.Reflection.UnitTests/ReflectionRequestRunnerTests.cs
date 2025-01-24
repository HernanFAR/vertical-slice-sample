using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using VSlices.Base;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Base.Definitions;
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

    public class BehaviorInterceptorOne<TRequest, TResponse> : IBehaviorInterceptor<TRequest, TResponse>
        where TRequest : IInput<TResponse>
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

    public class BehaviorInterceptorTwo<TRequest, TResponse> : IBehaviorInterceptor<TRequest, TResponse>
        where TRequest : IInput<TResponse>
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

    public class ConcreteBehaviorInterceptorOne : IBehaviorInterceptor<InputOne, Unit>
    {
        public Eff<VSlicesRuntime, Unit> Define(InputOne input, Eff<VSlicesRuntime, Unit> next) =>
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

    public record InputOne : IInput;

    public class RequestBehaviorOne : IBehavior<InputOne, Unit>
    {        
        public Eff<VSlicesRuntime, Unit> Define(InputOne inputOne) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() =>
            {
                accumulator.Count += 1;
                accumulator.Str += "HandlerOne_";

                return unit;
            })
            select unit;
    }

    public record InputTwo : IInput<Unit>;

    public class RequestBehaviorTwo : IBehavior<InputTwo, Unit>
    {
        public Eff<VSlicesRuntime, Unit> Define(InputTwo input) =>
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

        var services = new ServiceCollection()
            .AddVSlicesRuntime()
            .AddTransient<IRequestRunner, ReflectionRequestRunner>()
            .AddSingleton<Accumulator>();

        new FeatureComposer(services)
            .With<InputOne>().ExpectNoOutput()
            .ByExecuting<RequestBehaviorOne>();

        var provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new InputOne());

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

        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient<IRequestRunner, ReflectionRequestRunner>()
                       .AddSingleton<Accumulator>();

        new FeatureComposer(services)
            .With<InputOne>().ExpectNoOutput()
            .ByExecuting<RequestBehaviorOne>(chain => chain.Add(typeof(BehaviorInterceptorOne<,>)));

        var provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender      = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new InputOne());

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

        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient<IRequestRunner, ReflectionRequestRunner>()
                       .AddSingleton<Accumulator>();

        new FeatureComposer(services)
            .With<InputOne>().ExpectNoOutput()
            .ByExecuting<RequestBehaviorOne>(chain => chain
                                                      .Add(typeof(BehaviorInterceptorOne<,>))
                                                      .AddConcrete(typeof(ConcreteBehaviorInterceptorOne)));

        var provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new InputOne());

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

        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient<IRequestRunner, ReflectionRequestRunner>()
                       .AddSingleton<Accumulator>();

        new FeatureComposer(services)
            .With<InputOne>().ExpectNoOutput()
            .ByExecuting<RequestBehaviorOne>(chain => chain
                                                      .Add(typeof(BehaviorInterceptorOne<,>))
                                                      .Add(typeof(BehaviorInterceptorTwo<,>)));

        var provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new InputOne());

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

        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient<IRequestRunner, ReflectionRequestRunner>()
                       .AddSingleton<Accumulator>();

        new FeatureComposer(services)
            .With<InputOne>().ExpectNoOutput()
            .ByExecuting<RequestBehaviorOne>(chain => chain
                                                      .Add(typeof(BehaviorInterceptorOne<,>))
                                                      .Add(typeof(BehaviorInterceptorTwo<,>))
                                                      .AddConcrete(typeof(ConcreteBehaviorInterceptorOne)));

        var provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new InputOne());
        
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

        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient<IRequestRunner, ReflectionRequestRunner>()
                       .AddSingleton<Accumulator>();

        new FeatureComposer(services)
            .With<InputTwo>().ExpectNoOutput()
            .ByExecuting<RequestBehaviorTwo>(chain => chain
                                                      .Add(typeof(BehaviorInterceptorOne<,>))
                                                      .Add(typeof(BehaviorInterceptorTwo<,>)));

        var provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IRequestRunner>();

        Fin<Unit> effectResult = sender.Run(new InputTwo());

        _ = effectResult.Match(
            _ => unit,
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerTwo_");
        return Task.CompletedTask;
    }
}