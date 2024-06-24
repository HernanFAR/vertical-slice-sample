using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using VSlices.CrossCutting.StreamPipeline;
using static LanguageExt.Prelude;

namespace VSlices.Core.Stream.Reflection.UnitTests;

public class ReflectionStreamRunnerTests
{
    public sealed class Accumulator
    {
        public int Count { get; set; }

        public string Str { get; set; } = "";

    }

    public class PipelineBehaviorOne<TRequest, TResult> : IStreamPipelineBehavior<TRequest, TResult>
        where TRequest : IStream<TResult>
    {
        private readonly Accumulator _accumulator;

        public PipelineBehaviorOne(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<IAsyncEnumerable<TResult>> Define(
            TRequest request, 
            Aff<IAsyncEnumerable<TResult>> next, 
            CancellationToken cancellationToken = default) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "OpenPipelineOne_";

                return unit;
            })
            from result in next
            select result;
    }

    public class PipelineBehaviorTwo<TRequest, TResult> : IStreamPipelineBehavior<TRequest, TResult>
        where TRequest : IStream<TResult>
    {
        private readonly Accumulator _accumulator;

        public PipelineBehaviorTwo(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<IAsyncEnumerable<TResult>> Define(
            TRequest request, 
            Aff<IAsyncEnumerable<TResult>> next, 
            CancellationToken cancellationToken = default) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "OpenPipelineTwo_";

                return unit;
            })
            from result in next
            select result;
    }

    public class ConcretePipelineBehaviorOne : IStreamPipelineBehavior<Request, Response>
    {
        private readonly Accumulator _accumulator;

        public ConcretePipelineBehaviorOne(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<IAsyncEnumerable<Response>> Define(
            Request request, 
            Aff<IAsyncEnumerable<Response>> next,
            CancellationToken cancellationToken = default) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "ConcretePipelineOne_";

                return unit;
            })
            from result in next
            select result;
    }

    public record Response(int Value);
    public record Request : IStream<Response>;

    public class Handler : IStreamHandler<Request, Response>
    {
        private readonly Accumulator _accumulator;

        public Handler(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Aff<IAsyncEnumerable<Response>> Define(Request request, CancellationToken cancellationToken = default) =>
            from _ in Eff(() =>
            {
                _accumulator.Count += 1;
                _accumulator.Str += "HandlerOne_";

                return unit;
            })
            from result in Eff(() => Yield(request, cancellationToken))
            select result;

        public async IAsyncEnumerable<Response> Yield(
            Request request, 
            [EnumeratorCancellation] 
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(250, cancellationToken);
            yield return new Response(1);

            await Task.Delay(500, cancellationToken);
            yield return new Response(2);


            await Task.Delay(750, cancellationToken);
            yield return new Response(3);
        }
    }

    [Fact]
    public async Task Sender_Should_CallHandler()
    {
        const int expCount = 1;
        ServiceCollection services = new();

        services.AddTransient<IStreamHandler<Request, Response>, Handler>();
        services.AddTransient<IStreamRunner, ReflectionStreamRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IStreamRunner>();

        Fin<IAsyncEnumerable<Response>> result = await sender.RunAsync(new Request());

        await result.Match(
            async enumeration =>
            {
                var expValue = 1;

                await foreach (Response item in enumeration)
                {
                    item.Value.Should().Be(expValue);
                    expValue++;
                }
            },
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("HandlerOne_");

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndOpenPipeline()
    {
        const int expCount = 2;
        ServiceCollection services = new();

        services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient<IStreamHandler<Request, Response>, Handler>();
        services.AddTransient<IStreamRunner, ReflectionStreamRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();

        Accumulator accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IStreamRunner>();

        Fin<IAsyncEnumerable<Response>> result = await sender.RunAsync(new Request());

        await result.Match(
            async enumeration =>
            {
                var expValue = 1;

                await foreach (Response item in enumeration)
                {
                    item.Value.Should().Be(expValue);
                    expValue++;
                }
            },
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_HandlerOne_");

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndOpenPipelineAndClosedPipeline()
    {
        const int expCount = 3;
        ServiceCollection services = new();

        services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IStreamPipelineBehavior<Request, Response>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IStreamHandler<Request, Response>, Handler>();
        services.AddTransient<IStreamRunner, ReflectionStreamRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IStreamRunner>();

        Fin<IAsyncEnumerable<Response>> result = await sender.RunAsync(new Request());

        await result.Match(
            async enumeration =>
            {
                var expValue = 1;

                await foreach (Response item in enumeration)
                {
                    item.Value.Should().Be(expValue);
                    expValue++;
                }
            },
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_ConcretePipelineOne_HandlerOne_");
    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndTwoOpenPipeline()
    {
        const int expCount = 3;
        ServiceCollection services = new();

        services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient<IStreamHandler<Request, Response>, Handler>();
        services.AddTransient<IStreamRunner, ReflectionStreamRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();

        Accumulator accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IStreamRunner>();

        Fin<IAsyncEnumerable<Response>> result = await sender.RunAsync(new Request());

        await result.Match(
            async enumeration =>
            {
                var expValue = 1;

                await foreach (Response item in enumeration)
                {
                    item.Value.Should().Be(expValue);
                    expValue++;
                }
            },
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_HandlerOne_");

    }

    [Fact]
    public async Task Sender_Should_CallHandlerAndTwoOpenPipelineAndOneClosedPipeline()
    {
        const int expCount = 4;
        ServiceCollection services = new();

        services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(PipelineBehaviorOne<,>));
        services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(PipelineBehaviorTwo<,>));
        services.AddTransient(typeof(IStreamPipelineBehavior<Request, Response>), typeof(ConcretePipelineBehaviorOne));
        services.AddTransient<IStreamHandler<Request, Response>, Handler>();
        services.AddTransient<IStreamRunner, ReflectionStreamRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider provider = services.BuildServiceProvider();

        Accumulator accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IStreamRunner>();

        Fin<IAsyncEnumerable<Response>> result = await sender.RunAsync(new Request());

        await result.Match(
            async enumeration =>
            {
                var expValue = 1;

                await foreach (Response item in enumeration)
                {
                    item.Value.Should().Be(expValue);
                    expValue++;
                }
            },
            _ => throw new UnreachableException());

        accumulator.Count.Should().Be(expCount);
        accumulator.Str.Should().Be("OpenPipelineOne_OpenPipelineTwo_ConcretePipelineOne_HandlerOne_");

    }
}