using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.Events.Strategies;
using VSlices.CrossCutting.Pipeline;
using VSlices.Domain;

namespace VSlices.Core.Events._ReflectionPublisher.UnitTests;

public class ReflectionPublisherTests
{
    public class Acumulator
    {
        public static string Str { get; set; } = "";
    }

    public class PipelineBehaviorOne<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IFeature<TResponse>
    {
        public static int Count { get; set; }

        public ValueTask<Result<TResponse>> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
        {
            Count += 1;
            Acumulator.Str += "OpenPipelineOne_";

            return next();
        }
    }

    public class PipelineBehaviorTwo<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IFeature<TResponse>
    {
        public static int Count { get; set; }

        public ValueTask<Result<TResponse>> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
        {
            Count += 1;
            Acumulator.Str += "OpenPipelineTwo_";

            return next();
        }
    }

    public class ConcretePipelineBehaviorOne : IPipelineBehavior<RequestOne, Success>
    {
        public static int Count { get; set; }

        public ValueTask<Result<Success>> HandleAsync(RequestOne request, RequestHandlerDelegate<Success> next, CancellationToken cancellationToken = default)
        {
            Count += 1;
            Acumulator.Str += "ConcretePipelineOne_";

            return next();
        }
    }

    public record RequestOne : Event;

    public class HandlerOne : IHandler<RequestOne>
    {
        public static int Count { get; set; }

        public ValueTask<Result<Success>> HandleAsync(RequestOne requestOne, CancellationToken cancellationToken = default)
        {
            Count += 1;
            Acumulator.Str += "EventHandlerOne_";

            return ValueTask.FromResult<Result<Success>>(Success.Value);
        }
    }

    public record RequestTwo : Event;

    public class HandlerTwo : IHandler<RequestTwo>
    {
        public static int Count { get; set; }

        public ValueTask<Result<Success>> HandleAsync(RequestTwo request, CancellationToken cancellationToken = default)
        {
            Count += 1;
            Acumulator.Str += "EventHandlerTwo_";

            return ValueTask.FromResult<Result<Success>>(Success.Value);
        }
    }
    public record RequestThree : Event;

    public class RequestThreeHandlerA : IHandler<RequestThree>
    {
        public AutoResetEvent EventHandled { get; } = new(false);

        public async ValueTask<Result<Success>> HandleAsync(RequestThree request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000, default);

            EventHandled.Set();

            return Success.Value;
        }
    }

    public class RequestThreeHandlerB : IHandler<RequestThree>
    {
        public AutoResetEvent EventHandled { get; } = new(false);

        public async ValueTask<Result<Success>> HandleAsync(RequestThree request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(2000, default);

            EventHandled.Set();

            return Success.Value;
        }
    }

    [Fact]
    public async Task Publisher_Should_CallOneHandler()
    {
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddTransient<IPublisher, ReflectionPublisher>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IPublisher>();

        await publisher.PublishAsync(new RequestOne(), default);

        Acumulator.Str.Should().Be("EventHandlerOne_");
        HandlerOne.Count.Should().Be(expCount);

        HandlerOne.Count = 0;
        Acumulator.Str = "";

    }

    [Theory]
    [InlineData(typeof(AwaitForEachStrategy), 2999)]
    [InlineData(typeof(AwaitInParallelStrategy), 1999)]
    public async Task Publisher_Should_CallManyHandler(Type strategyType, int time)
    {
        ReflectionPublisher.RequestHandlers.Clear();

        var services = new ServiceCollection();
        var strategy = (IPublishingStrategy)Activator.CreateInstance(strategyType)!;

        services.AddScoped<RequestThreeHandlerA>();
        services.AddScoped<IHandler<RequestThree, Success>>(s => s.GetRequiredService<RequestThreeHandlerA>());
        services.AddScoped<RequestThreeHandlerB>();
        services.AddScoped<IHandler<RequestThree, Success>>(s => s.GetRequiredService<RequestThreeHandlerB>());
        services.AddScoped<IPublisher, ReflectionPublisher>();
        services.AddScoped(_ => strategy);

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IPublisher>();

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
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddScoped<IPublisher, ReflectionPublisher>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IPublisher>();

        await publisher.PublishAsync(new RequestOne(), default);

        PipelineBehaviorOne<RequestOne, Success>.Count.Should().Be(expCount);
        HandlerOne.Count.Should().Be(expCount);
        Acumulator.Str.Should().Be("OpenPipelineOne_EventHandlerOne_");

        PipelineBehaviorOne<RequestOne, Success>.Count = 0;
        HandlerOne.Count = 0;
        Acumulator.Str = "";

    }

    [Fact]
    public async Task Publisher_Should_CallHandlerAndOpenPipelineAndClosedPipeline()
    {
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Success>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddTransient<IPublisher, ReflectionPublisher>();
        services.AddScoped<IPublisher, ReflectionPublisher>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IPublisher>();

        await publisher.PublishAsync(new RequestOne(), default);

        PipelineBehaviorOne<RequestOne, Success>.Count.Should().Be(expCount);
        ConcretePipelineBehaviorOne.Count.Should().Be(expCount);
        HandlerOne.Count.Should().Be(expCount);
        Acumulator.Str.Should().Be("OpenPipelineOne_ConcretePipelineOne_EventHandlerOne_");

        PipelineBehaviorOne<RequestOne, Success>.Count = 0;
        ConcretePipelineBehaviorOne.Count = 0;
        HandlerOne.Count = 0;
        Acumulator.Str = "";

    }

    [Fact]
    public async Task Publisher_Should_CallHandlerAndTwoOpenPipeline()
    {
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddScoped<IPublisher, ReflectionPublisher>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IPublisher>();

        await publisher.PublishAsync(new RequestOne(), default);

        PipelineBehaviorOne<RequestOne, Success>.Count.Should().Be(expCount);
        PipelineBehaviorTwo<RequestOne, Success>.Count.Should().Be(expCount);
        HandlerOne.Count.Should().Be(expCount);
        Acumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerOne_");

        PipelineBehaviorOne<RequestOne, Success>.Count = 0;
        PipelineBehaviorTwo<RequestOne, Success>.Count = 0;
        HandlerOne.Count = 0;
        Acumulator.Str = "";

    }

    [Fact]
    public async Task Publisher_Should_CallHandlerAndTwoOpenPipelineAndOneClosedPipeline()
    {
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Success>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddScoped<IPublisher, ReflectionPublisher>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();

        var provider = services.BuildServiceProvider();
        var publish = provider.GetRequiredService<IPublisher>();

        await publish.PublishAsync(new RequestOne(), default);

        PipelineBehaviorOne<RequestOne, Success>.Count.Should().Be(expCount);
        PipelineBehaviorTwo<RequestOne, Success>.Count.Should().Be(expCount);
        ConcretePipelineBehaviorOne.Count.Should().Be(expCount);
        HandlerOne.Count.Should().Be(expCount);
        Acumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_ConcretePipelineOne_EventHandlerOne_");

        PipelineBehaviorOne<RequestOne, Success>.Count = 0;
        PipelineBehaviorTwo<RequestOne, Success>.Count = 0;
        ConcretePipelineBehaviorOne.Count = 0;
        HandlerOne.Count = 0;
        Acumulator.Str = "";

    }

    [Fact]
    public async Task Publisher_Should_CallHandlerAndTwoOpenPipelineAndNoneClosedPipeline()
    {
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Success>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddTransient<IHandler<RequestTwo, Success>, HandlerTwo>();
        services.AddScoped<IPublisher, ReflectionPublisher>();
        services.AddScoped<IPublishingStrategy, AwaitForEachStrategy>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IPublisher>();

        await sender.PublishAsync(new RequestTwo(), default);

        PipelineBehaviorOne<RequestTwo, Success>.Count.Should().Be(expCount);
        PipelineBehaviorTwo<RequestTwo, Success>.Count.Should().Be(expCount);
        ConcretePipelineBehaviorOne.Count.Should().Be(0);
        HandlerTwo.Count.Should().Be(expCount);
        Acumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_EventHandlerTwo_");

        PipelineBehaviorOne<RequestTwo, Success>.Count = 0;
        PipelineBehaviorTwo<RequestTwo, Success>.Count = 0;
        HandlerTwo.Count = 0;
        Acumulator.Str = "";

    }
}