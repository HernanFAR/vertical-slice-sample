using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using static LanguageExt.Prelude;
using static VSlices.VSlicesPrelude;
using VSlices.Base;
using VSlices.Core.Events.Strategies;
using VSlices.Domain;
using VSlices.Base.Traits;
using VSlices.Core.UseCases;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Events._ReflectionRunner.UnitTests;

public class ReflectionEventRunnerTests
{
    public sealed class Accumulator
    {
        public string Str { get; set; } = "";
        
        public int Count { get; set; }

    }

    public sealed class PipelineBehaviorOne<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IFeature<TResponse>
    {
        public Eff<VSlicesRuntime, TResponse> Define(TRequest request, Eff<VSlicesRuntime, TResponse> next) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() =>
            {
                accumulator.Count += 1;
                accumulator.Str   += "OpenPipelineOne_";

                return unit;
            })
            from result in next
            select result;
    }

    public sealed class PipelineBehaviorTwo<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IFeature<TResponse>
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

    public sealed class ConcretePipelineBehaviorOne : IPipelineBehavior<RequestOne, Unit>
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

    public sealed record RequestOne : Event;

    public sealed class RequestHandlerOne : IEventHandler<RequestOne>
    {
        public Eff<VSlicesRuntime, Unit> Define(RequestOne requestOne) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() =>
            {
                accumulator.Count += 1;
                accumulator.Str += "EventHandlerOne_";

                return unit;
            })
            select unit;
    }

    public sealed record RequestTwo : Event;

    public sealed class RequestHandlerTwo : IEventHandler<RequestTwo>
    {
        public Eff<VSlicesRuntime, Unit> Define(RequestTwo request) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() =>
            {
                accumulator.Count += 1;
                accumulator.Str   += "EventHandlerTwo_";

                return unit;
            })
            select unit;
    }
    public sealed record RequestThree : Event;

    public sealed class RequestThreeRequestHandlerA : IEventHandler<RequestThree>
    {
        public AutoResetEvent EventHandled { get; } = new(false);

        public Eff<VSlicesRuntime, Unit> Define(RequestThree request) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(async () =>
            {
                await Task.Delay(1000, default);
                EventHandled.Set();

                return unit;
            })
            select unit;
    }

    public sealed class RequestThreeRequestHandlerB : IEventHandler<RequestThree>
    {
        public AutoResetEvent EventHandled { get; } = new(false);

        public Eff<VSlicesRuntime, Unit> Define(RequestThree request) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(async () =>
            {
                await Task.Delay(2000, default);
                EventHandled.Set();

                return unit;
            })
            select unit;
    }

    [Fact]
    public Task Publisher_Should_CallOneHandler()
    {
        const int expCount = 1;
        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient<IEventHandler<RequestOne>, RequestHandlerOne>()
                       .AddTransient<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new RequestOne(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("EventHandlerOne_");
        return Task.CompletedTask;
    }

    [Theory]
    [InlineData(typeof(AwaitForEachStrategy), 2999)]
    public Task Publisher_Should_CallManyHandler(Type strategyType, int time)
    {
        var strategy = (IPublishingStrategy)Activator.CreateInstance(strategyType)!;

        var provider = new ServiceCollection()
            .AddVSlicesRuntime()
            .AddScoped<RequestThreeRequestHandlerA>()
            .AddScoped<IEventHandler<RequestThree>>(s => s.GetRequiredService<RequestThreeRequestHandlerA>())
            .AddScoped<RequestThreeRequestHandlerB>()
            .AddScoped<IEventHandler<RequestThree>>(s => s.GetRequiredService<RequestThreeRequestHandlerB>())
            .AddScoped<IEventRunner, ReflectionEventRunner>()
            .AddSingleton<Accumulator>()
            .AddScoped(_ => strategy)
            .BuildServiceProvider();

        var publisher = provider.GetRequiredService<IEventRunner>();

        var stopwatch = Stopwatch.StartNew();

        publisher.Publish(new RequestThree(), default);

        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(time);

        var handlerA = provider.GetRequiredService<RequestThreeRequestHandlerA>();
        var handlerB = provider.GetRequiredService<RequestThreeRequestHandlerB>();

        handlerA.EventHandled.WaitOne(1000)
            .Should().BeTrue();
        handlerB.EventHandled.WaitOne(1000)
            .Should().BeTrue();
        return Task.CompletedTask;
    }

    [Fact]
    public Task Publisher_Should_CallHandlerAndOpenPipeline()
    {
        const int expCount = 2;
        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>))
                       .AddTransient<IEventHandler<RequestOne>, RequestHandlerOne>()
                       .AddScoped<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new RequestOne(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_EventHandlerOne_");

        return Task.CompletedTask;
    }

    [Fact]
    public Task Publisher_Should_CallHandlerAndOpenPipelineAndClosedPipeline()
    {
        const int expCount = 3;
        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>))
                       .AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne))
                       .AddTransient<IEventHandler<RequestOne>, RequestHandlerOne>()
                       .AddTransient<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();
        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new RequestOne(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_ConcretePipelineOne_EventHandlerOne_");
        return Task.CompletedTask;
    }

    [Fact]
    public Task Publisher_Should_CallHandlerAndTwoOpenPipeline()
    {
        const int expCount = 3;

        ServiceProvider provider = new ServiceCollection()
                                   .AddVSlicesRuntime()
                                   .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>))
                                   .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>))
                                   .AddTransient<IEventHandler<RequestOne>, RequestHandlerOne>()
                                   .AddScoped<IEventRunner, ReflectionEventRunner>()
                                   .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                                   .AddSingleton<Accumulator>()
                                   .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new RequestOne(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerOne_");
        return Task.CompletedTask;
    }

    [Fact]
    public Task Publisher_Should_CallHandlerAndTwoOpenPipelineAndOneClosedPipeline()
    {
        const int expCount = 4;
        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>))
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>))
                       .AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne))
                       .AddTransient<IEventHandler<RequestOne>, RequestHandlerOne>()
                       .AddScoped<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new RequestOne(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_ConcretePipelineOne_EventHandlerOne_");
        return Task.CompletedTask;
    }

    [Fact]
    public Task Publisher_Should_CallHandlerAndTwoOpenPipelineAndNoneClosedPipeline()
    {
        const int expCount = 3;

        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>))
                       .AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>))
                       .AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne))
                       .AddTransient<IEventHandler<RequestOne>, RequestHandlerOne>()
                       .AddTransient<IEventHandler<RequestTwo>, RequestHandlerTwo>()
                       .AddScoped<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new RequestTwo(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerTwo_");
        return Task.CompletedTask;
    }
}