using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using static LanguageExt.Prelude;
using static VSlices.VSlicesPrelude;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.Core.Events.Strategies;
using VSlices.Domain;

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

    public sealed class ConcretePipelineBehaviorOne : IPipelineBehavior<EventOne, Unit>
    {
        public Eff<VSlicesRuntime, Unit> Define(EventOne @event, Eff<VSlicesRuntime, Unit> next) =>
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

    public sealed record EventOne : Event;

    public sealed class HandlerOne : IHandler<EventOne>
    {
        public Eff<VSlicesRuntime, Unit> Define(EventOne input) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() =>
            {
                accumulator.Count += 1;
                accumulator.Str += "EventHandlerOne_";

                return unit;
            })
            select unit;
    }

    public sealed record EventTwo : Event;

    public sealed class HandlerTwo : IHandler<EventTwo>
    {
        public Eff<VSlicesRuntime, Unit> Define(EventTwo input) =>
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

    public sealed class RequestThreeRequestHandlerA : IHandler<RequestThree>
    {
        public AutoResetEvent EventHandled { get; } = new(false);

        public Eff<VSlicesRuntime, Unit> Define(RequestThree input) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(async () =>
            {
                await Task.Delay(1000, default);
                EventHandled.Set();

                return unit;
            })
            select unit;
    }

    public sealed class RequestThreeRequestHandlerB : IHandler<RequestThree>
    {
        public AutoResetEvent EventHandled { get; } = new(false);

        public Eff<VSlicesRuntime, Unit> Define(RequestThree input) =>
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
    public void Publisher_Should_CallOneHandler()
    {
        // Arrange
        const int expCount = 1;

        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddTransient<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                       .AddSingleton<Accumulator>();

        new FeatureDefinition<EventOne, Unit>(services)
            .Execute<HandlerOne>();

        var provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        // Act
        var result = publisher.Publish(new EventOne());

        // Assert
        result.IsSucc.Should().BeTrue();

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("EventHandlerOne_");
    }

    [Theory]
    [InlineData(typeof(AwaitForEachStrategy), 2999)]
    public void Publisher_Should_CallManyHandler(Type strategyType, int time)
    {
        var provider = new ServiceCollection()
            .AddVSlicesRuntime()
            .AddScoped<RequestThreeRequestHandlerA>()
            .AddScoped<IHandler<RequestThree, Unit>>(s => s.GetRequiredService<RequestThreeRequestHandlerA>())
            .AddScoped<RequestThreeRequestHandlerB>()
            .AddScoped<IHandler<RequestThree, Unit>>(s => s.GetRequiredService<RequestThreeRequestHandlerB>())
            .AddScoped<IEventRunner, ReflectionEventRunner>()
            .AddSingleton<Accumulator>()
            .AddScoped(typeof(IPublishingStrategy), strategyType)
            .BuildServiceProvider();

        var publisher = provider.GetRequiredService<IEventRunner>();

        var stopwatch = Stopwatch.StartNew();

        var result = publisher.Publish(new RequestThree());

        stopwatch.Stop();

        result.IsSucc.Should().BeTrue();

        stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(time);

        var handlerA = provider.GetRequiredService<RequestThreeRequestHandlerA>();
        var handlerB = provider.GetRequiredService<RequestThreeRequestHandlerB>();

        handlerA.EventHandled.WaitOne(1000)
            .Should().BeTrue();
        handlerB.EventHandled.WaitOne(1000)
            .Should().BeTrue();
    }

    [Fact]
    public void Publisher_Should_CallHandlerAndOpenPipeline()
    {
        const int expCount = 2;

        var behaviorChain = new HandlerBehaviorChain<HandlerOne>([typeof(PipelineBehaviorOne<EventOne, Unit>)]);

        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddSingleton(behaviorChain)
                       .AddTransient(typeof(PipelineBehaviorOne<,>))
                       .AddTransient<IHandler<EventOne, Unit>, HandlerOne>()
                       .AddScoped<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new EventOne());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_EventHandlerOne_");

    }

    [Fact]
    public void Publisher_Should_CallHandlerAndOpenPipelineAndClosedPipeline()
    {
        const int expCount = 3;

        var behaviorChain = new HandlerBehaviorChain<HandlerOne>([
            typeof(PipelineBehaviorOne<EventOne, Unit>), 
            typeof(ConcretePipelineBehaviorOne)
        ]);

        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddSingleton(behaviorChain)
                       .AddTransient(typeof(PipelineBehaviorOne<,>))
                       .AddTransient(typeof(ConcretePipelineBehaviorOne))
                       .AddTransient<IHandler<EventOne, Unit>, HandlerOne>()
                       .AddTransient<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new EventOne());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_ConcretePipelineOne_EventHandlerOne_");
    }

    [Fact]
    public void Publisher_Should_CallHandlerAndTwoOpenPipeline()
    {
        const int expCount = 3;

        var behaviorChain = new HandlerBehaviorChain<HandlerOne>([
            typeof(PipelineBehaviorOne<EventOne, Unit>),
            typeof(PipelineBehaviorTwo<EventOne, Unit>)
        ]);

        ServiceProvider provider = new ServiceCollection()
                                   .AddVSlicesRuntime()
                                   .AddSingleton(behaviorChain)
                                   .AddTransient(typeof(PipelineBehaviorOne<,>))
                                   .AddTransient(typeof(PipelineBehaviorTwo<,>))
                                   .AddTransient<IHandler<EventOne, Unit>, HandlerOne>()
                                   .AddScoped<IEventRunner, ReflectionEventRunner>()
                                   .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                                   .AddSingleton<Accumulator>()
                                   .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new EventOne());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerOne_");
    }

    [Fact]
    public void Publisher_Should_CallHandlerAndTwoOpenPipelineAndOneClosedPipeline()
    {
        const int expCount = 4;

        var behaviorChain = new HandlerBehaviorChain<HandlerOne>([
            typeof(PipelineBehaviorOne<EventOne, Unit>),
            typeof(PipelineBehaviorTwo<EventOne, Unit>),
            typeof(ConcretePipelineBehaviorOne)
        ]);

        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddSingleton(behaviorChain)
                       .AddTransient(typeof(PipelineBehaviorOne<,>))
                       .AddTransient(typeof(PipelineBehaviorTwo<,>))
                       .AddTransient(typeof(ConcretePipelineBehaviorOne))
                       .AddTransient<IHandler<EventOne, Unit>, HandlerOne>()
                       .AddScoped<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new EventOne());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_ConcretePipelineOne_EventHandlerOne_");
    }

    [Fact]
    public void Publisher_Should_CallHandlerAndTwoOpenPipelineAndNoneClosedPipeline()
    {
        const int expCount = 3;

        var behaviorChain = new HandlerBehaviorChain<HandlerTwo>([
            typeof(PipelineBehaviorOne<EventTwo, Unit>),
            typeof(PipelineBehaviorTwo<EventTwo, Unit>)
        ]);

        var provider = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddSingleton(behaviorChain)
                       .AddTransient(typeof(PipelineBehaviorOne<,>))
                       .AddTransient(typeof(PipelineBehaviorTwo<,>))
                       .AddTransient(typeof(ConcretePipelineBehaviorOne))
                       .AddTransient<IHandler<EventOne, Unit>, HandlerOne>()
                       .AddTransient<IHandler<EventTwo, Unit>, HandlerTwo>()
                       .AddScoped<IEventRunner, ReflectionEventRunner>()
                       .AddScoped<IPublishingStrategy, AwaitForEachStrategy>()
                       .AddSingleton<Accumulator>()
                       .BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        publisher.Publish(new EventTwo());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerTwo_");
    }
}