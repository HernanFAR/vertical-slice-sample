using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.UseCases;
using VSlices.CrossCutting;

namespace VSlices.Core.Sender.Reflection.IntegTests;

public class ReflectionSenderTests
{
    public class Acumulator
    {
        public static string Str { get; set; } = "";
    }

    public class PipelineBehaviorOne<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
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
        where TRequest : IRequest<TResponse>
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

    public record RequestOne : IRequest<Success>;

    public class HandlerOne : IHandler<RequestOne, Success>
    {
        public static int Count { get; set; }

        public ValueTask<Result<Success>> HandleAsync(RequestOne requestOne, CancellationToken cancellationToken = default)
        {
            Count += 1;
            Acumulator.Str += "HandlerOne_";

            return ValueTask.FromResult<Result<Success>>(Success.Value);
        }
    }

    public record RequestTwo : IRequest<Success>;

    public class HandlerTwo : IHandler<RequestTwo, Success>
    {
        public static int Count { get; set; }

        public ValueTask<Result<Success>> HandleAsync(RequestTwo request, CancellationToken cancellationToken = default)
        {
            Count += 1;
            Acumulator.Str += "EventHandlerTwo_";

            return ValueTask.FromResult<Result<Success>>(Success.Value);
        }
    }

    [Fact]
    public async Task Sender_Should_CallHandler()
    {
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddTransient<ISender, ReflectionSender>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var response = await sender.SendAsync<Success>(new RequestOne(), default);

        response.IsSuccess.Should().BeTrue();

        Acumulator.Str.Should().Be("HandlerOne_");
        HandlerOne.Count.Should().Be(expCount);

        HandlerOne.Count = 0;
        Acumulator.Str = "";

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndOpenPipeline()
    {
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddTransient<ISender, ReflectionSender>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var response = await sender.SendAsync(new RequestOne(), default);

        response.IsSuccess.Should().BeTrue();

        PipelineBehaviorOne<RequestOne, Success>.Count.Should().Be(expCount);
        HandlerOne.Count.Should().Be(expCount);
        Acumulator.Str.Should().Be("OpenPipelineOne_HandlerOne_");

        PipelineBehaviorOne<RequestOne, Success>.Count = 0;
        HandlerOne.Count = 0;
        Acumulator.Str = "";

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndOpenPipelineAndClosedPipeline()
    {
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Success>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddTransient<ISender, ReflectionSender>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var response = await sender.SendAsync(new RequestOne(), default);

        response.IsSuccess.Should().BeTrue();

        PipelineBehaviorOne<RequestOne, Success>.Count.Should().Be(expCount);
        ConcretePipelineBehaviorOne.Count.Should().Be(expCount);
        HandlerOne.Count.Should().Be(expCount);
        Acumulator.Str.Should().Be("OpenPipelineOne_ConcretePipelineOne_HandlerOne_");

        PipelineBehaviorOne<RequestOne, Success>.Count = 0;
        ConcretePipelineBehaviorOne.Count = 0;
        HandlerOne.Count = 0;
        Acumulator.Str = "";

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndTwoOpenPipeline()
    {
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddTransient<ISender, ReflectionSender>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var response = await sender.SendAsync(new RequestOne(), default);

        response.IsSuccess.Should().BeTrue();

        PipelineBehaviorOne<RequestOne, Success>.Count.Should().Be(expCount);
        PipelineBehaviorTwo<RequestOne, Success>.Count.Should().Be(expCount);
        HandlerOne.Count.Should().Be(expCount);
        Acumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_HandlerOne_");

        PipelineBehaviorOne<RequestOne, Success>.Count = 0;
        PipelineBehaviorTwo<RequestOne, Success>.Count = 0;
        HandlerOne.Count = 0;
        Acumulator.Str = "";

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndTwoOpenPipelineAndOneClosedPipeline()
    {
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Success>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddTransient<ISender, ReflectionSender>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var response = await sender.SendAsync(new RequestOne(), default);

        response.IsSuccess.Should().BeTrue();

        PipelineBehaviorOne<RequestOne, Success>.Count.Should().Be(expCount);
        PipelineBehaviorTwo<RequestOne, Success>.Count.Should().Be(expCount);
        ConcretePipelineBehaviorOne.Count.Should().Be(expCount);
        HandlerOne.Count.Should().Be(expCount);
        Acumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_ConcretePipelineOne_HandlerOne_");

        PipelineBehaviorOne<RequestOne, Success>.Count = 0;
        PipelineBehaviorTwo<RequestOne, Success>.Count = 0;
        ConcretePipelineBehaviorOne.Count = 0;
        HandlerOne.Count = 0;
        Acumulator.Str = "";

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndTwoOpenPipelineAndNoneClosedPipeline()
    {
        const int expCount = 1;
        var services = new ServiceCollection();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient(typeof(IPipelineBehavior<RequestOne, Success>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IHandler<RequestOne, Success>, HandlerOne>();
        services.AddTransient<IHandler<RequestTwo, Success>, HandlerTwo>();
        services.AddTransient<ISender, ReflectionSender>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var response = await sender.SendAsync(new RequestTwo(), default);

        response.IsSuccess.Should().BeTrue();

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