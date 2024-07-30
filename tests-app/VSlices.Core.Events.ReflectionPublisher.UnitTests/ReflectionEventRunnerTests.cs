using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using static LanguageExt.Prelude;
using static VSlices.CorePrelude;
using VSlices.Base;
using VSlices.Core.Events.Strategies;
using VSlices.CrossCutting.Pipeline;
using VSlices.Domain;
using VSlices.Core.Traits;

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
        public Eff<HandlerRuntime, TResponse> Define(TRequest request, Eff<HandlerRuntime, TResponse> next) =>
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
        public Eff<HandlerRuntime, TResponse> Define(TRequest request, Eff<HandlerRuntime, TResponse> next) =>
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
        public Eff<HandlerRuntime, Unit> Define(RequestOne request, Eff<HandlerRuntime, Unit> next) =>
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

    public sealed class HandlerOne : IHandler<RequestOne>
    {
        public Eff<HandlerRuntime, Unit> Define(RequestOne requestOne) =>
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

    public sealed class HandlerTwo : IHandler<RequestTwo>
    {
        public Eff<HandlerRuntime, Unit> Define(RequestTwo request) =>
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

    public sealed class RequestThreeHandlerA : IHandler<RequestThree>
    {
        public AutoResetEvent EventHandled { get; } = new(false);

        public Eff<HandlerRuntime, Unit> Define(RequestThree request) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(async () =>
            {
                await Task.Delay(1000, default);
                EventHandled.Set();

                return unit;
            })
            select unit;
    }

    public sealed class RequestThreeHandlerB : IHandler<RequestThree>
    {
        public AutoResetEvent EventHandled { get; } = new(false);

        public Eff<HandlerRuntime, Unit> Define(RequestThree request) =>
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
        var services = new ServiceCollection();

        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddTransient<IEventRunner, ReflectionEventRunner>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();
        services.AddSingleton<Accumulator>();

        var provider = services.BuildServiceProvider();
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
        var services = new ServiceCollection();
        var strategy = (IPublishingStrategy)Activator.CreateInstance(strategyType)!;

        services.AddScoped<RequestThreeHandlerA>();
        services.AddScoped<IHandler<RequestThree, Unit>>(s => s.GetRequiredService<RequestThreeHandlerA>());
        services.AddScoped<RequestThreeHandlerB>();
        services.AddScoped<IHandler<RequestThree, Unit>>(s => s.GetRequiredService<RequestThreeHandlerB>());
        services.AddScoped<IEventRunner, ReflectionEventRunner>();
        services.AddSingleton<Accumulator>();
        services.AddScoped(_ => strategy);

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IEventRunner>();

        var stopwatch = Stopwatch.StartNew();

        publisher.Publish(new RequestThree(), default);

        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(time);

        var handlerA = provider.GetRequiredService<RequestThreeHandlerA>();
        var handlerB = provider.GetRequiredService<RequestThreeHandlerB>();

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
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddScoped<IEventRunner, ReflectionEventRunner>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();
        services.AddSingleton<Accumulator>();

        var provider = services.BuildServiceProvider();
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
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddTransient<IEventRunner, ReflectionEventRunner>();
        services.AddScoped<IEventRunner, ReflectionEventRunner>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();
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
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddScoped<IEventRunner, ReflectionEventRunner>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();
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
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddScoped<IEventRunner, ReflectionEventRunner>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();
        services.AddSingleton<Accumulator>();

        var provider = services.BuildServiceProvider();
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
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Unit>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Unit>, HandlerOne>();
        services.AddTransient<IHandler<RequestTwo, Unit>, HandlerTwo>();
        services.AddScoped<IEventRunner, ReflectionEventRunner>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();
        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new RequestTwo(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerTwo_");
        return Task.CompletedTask;
    }
}