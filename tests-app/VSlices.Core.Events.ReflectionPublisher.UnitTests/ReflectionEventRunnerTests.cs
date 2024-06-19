using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Core.Events.Strategies;
using VSlices.CrossCutting.Pipeline;
using VSlices.Domain;
using static System.Net.Mime.MediaTypeNames;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Events._ReflectionPublisher.UnitTests;

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
        readonly Accumulator _accumulator;

        public PipelineBehaviorOne(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<TResponse> Define(TRequest request, Aff<TResponse> next, CancellationToken cancellationToken = default) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "OpenPipelineOne_";

                return unit;
            })
            from result in next
            select result;
    }

    public sealed class PipelineBehaviorTwo<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IFeature<TResponse>
    {
        readonly Accumulator _accumulator;

        public PipelineBehaviorTwo(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<TResponse> Define(TRequest request, Aff<TResponse> next, CancellationToken cancellationToken = default) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "OpenPipelineTwo_";

                return unit;
            })
            from result in next
            select result;
    }

    public sealed class ConcretePipelineBehaviorOne : IPipelineBehavior<RequestOne, Unit>
    {
        readonly Accumulator _accumulator;

        public ConcretePipelineBehaviorOne(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<Unit> Define(RequestOne request, Aff<Unit> next, CancellationToken cancellationToken = default) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "ConcretePipelineOne_";

                return unit;
            })
            from result in next
            select result;
    }

    public sealed record RequestOne : Event;

    public sealed class HandlerOne : IHandler<RequestOne>
    {
        readonly Accumulator _accumulator;

        public HandlerOne(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<Unit> Define(RequestOne requestOne, CancellationToken cancellationToken = default) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "EventHandlerOne_";

                return unit;
            })
            select unit;
    }

    public sealed record RequestTwo : Event;

    public sealed class HandlerTwo : IHandler<RequestTwo>
    {
        readonly Accumulator _accumulator;

        public HandlerTwo(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<Unit> Define(RequestTwo request, CancellationToken cancellationToken = default) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "EventHandlerTwo_";

                return unit;
            })
            select unit;
    }
    public sealed record RequestThree : Event;

    public sealed class RequestThreeHandlerA : IHandler<RequestThree>
    {
        readonly Accumulator _accumulator;

        public RequestThreeHandlerA(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public AutoResetEvent EventHandled { get; } = new(false);

        public Aff<Unit> Define(RequestThree request, CancellationToken cancellationToken = default) =>
            from _ in Aff(async () =>
            {
                await Task.Delay(1000, default);
                EventHandled.Set();

                return unit;
            })
            select unit;
    }

    public sealed class RequestThreeHandlerB : IHandler<RequestThree>
    {
        readonly Accumulator _accumulator;

        public RequestThreeHandlerB(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public AutoResetEvent EventHandled { get; } = new(false);

        public Aff<Unit> Define(RequestThree request, CancellationToken cancellationToken = default) =>
            from _ in Aff(async () =>
            {
                await Task.Delay(2000, default);
                EventHandled.Set();

                return unit;
            })
            select unit;
    }

    [Fact]
    public async Task Publisher_Should_CallOneHandler()
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

        await publisher.PublishAsync(new RequestOne(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("EventHandlerOne_");

    }

    [Theory]
    [InlineData(typeof(AwaitForEachStrategy), 2999)]
    [InlineData(typeof(AwaitInParallelStrategy), 1999)]
    public async Task Publisher_Should_CallManyHandler(Type strategyType, int time)
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
        var accumulator = provider.GetRequiredService<Accumulator>();
        var publisher = provider.GetRequiredService<IEventRunner>();

        var stopwatch = Stopwatch.StartNew();
        await publisher.PublishAsync(new RequestThree(), default);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(time);

        var handlerA = provider.GetRequiredService<RequestThreeHandlerA>();
        var handlerB = provider.GetRequiredService<RequestThreeHandlerB>();

        handlerA.EventHandled.WaitOne(1000)
            .Should().BeTrue();
        handlerB.EventHandled.WaitOne(1000)
            .Should().BeTrue();


    }

    [Fact]
    public async Task Publisher_Should_CallHandlerAndOpenPipeline()
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

        await publisher.PublishAsync(new RequestOne(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_EventHandlerOne_");

    }

    [Fact]
    public async Task Publisher_Should_CallHandlerAndOpenPipelineAndClosedPipeline()
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

        await publisher.PublishAsync(new RequestOne(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_ConcretePipelineOne_EventHandlerOne_");

    }

    [Fact]
    public async Task Publisher_Should_CallHandlerAndTwoOpenPipeline()
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

        await publisher.PublishAsync(new RequestOne(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerOne_");

    }

    [Fact]
    public async Task Publisher_Should_CallHandlerAndTwoOpenPipelineAndOneClosedPipeline()
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
        var publish = provider.GetRequiredService<IEventRunner>();

        await publish.PublishAsync(new RequestOne(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_ConcretePipelineOne_EventHandlerOne_");

    }

    [Fact]
    public async Task Publisher_Should_CallHandlerAndTwoOpenPipelineAndNoneClosedPipeline()
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
        var sender = provider.GetRequiredService<IEventRunner>();

        await sender.PublishAsync(new RequestTwo(), default);

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerTwo_");

    }
}