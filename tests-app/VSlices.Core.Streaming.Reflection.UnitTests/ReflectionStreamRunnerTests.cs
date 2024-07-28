using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using VSlices.Core.Traits;
using VSlices.CrossCutting.StreamPipeline;
using static LanguageExt.Prelude;
using static VSlices.CorePrelude;

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
        public Eff<HandlerRuntime, IAsyncEnumerable<TResult>> Define(
            TRequest request,
            Eff<HandlerRuntime, IAsyncEnumerable<TResult>> next) =>
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

    public class PipelineBehaviorTwo<TRequest, TResult> : IStreamPipelineBehavior<TRequest, TResult>
        where TRequest : IStream<TResult>
    {
        private readonly Accumulator _accumulator;

        public PipelineBehaviorTwo(Accumulator accumulator)
        {
            _accumulator = accumulator;
        }

        public Eff<HandlerRuntime, IAsyncEnumerable<TResult>> Define(
            TRequest request,
            Eff<HandlerRuntime, IAsyncEnumerable<TResult>> next) =>
            from _ in liftEff(() =>
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

        public Eff<HandlerRuntime, IAsyncEnumerable<Response>> Define(
            Request request,
            Eff<HandlerRuntime, IAsyncEnumerable<Response>> next) =>
            from _ in liftEff(() =>
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

        public Eff<HandlerRuntime, IAsyncEnumerable<Response>> Define(Request request) =>
            from token in cancelToken
            from _ in liftEff(() =>
                {
                    _accumulator.Count += 1;
                    _accumulator.Str += "HandlerOne_";

                    return unit;
                })
            from result in liftEff(() => Yield(request, token))
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
        ServiceCollection services = [];

        services.AddTransient<IStreamHandler<Request, Response>, Handler>();
        services.AddTransient<IStreamRunner, ReflectionStreamRunner>();
        services.AddSingleton<Accumulator>();

        ServiceProvider    provider           = services.BuildServiceProvider();
        DependencyProvider dependencyProvider = new(provider);

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IStreamRunner>();

        Fin<IAsyncEnumerable<Response>> result = sender.Run(new Request(), HandlerRuntime.New(dependencyProvider, EnvIO.New()));

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

        ServiceProvider    provider           = services.BuildServiceProvider();
        DependencyProvider dependencyProvider = new(provider);

        Accumulator accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IStreamRunner>();

        Fin<IAsyncEnumerable<Response>> result = sender.Run(new Request(), HandlerRuntime.New(dependencyProvider, EnvIO.New()));

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

        ServiceProvider    provider           = services.BuildServiceProvider();
        DependencyProvider dependencyProvider = new(provider);

        var accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IStreamRunner>();

        Fin<IAsyncEnumerable<Response>> result = sender.Run(new Request(), HandlerRuntime.New(dependencyProvider, EnvIO.New()));

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

        ServiceProvider    provider           = services.BuildServiceProvider();
        DependencyProvider dependencyProvider = new(provider);

        Accumulator accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IStreamRunner>();

        Fin<IAsyncEnumerable<Response>> result = sender.Run(new Request(), HandlerRuntime.New(dependencyProvider, EnvIO.New()));

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

        ServiceProvider    provider           = services.BuildServiceProvider();
        DependencyProvider dependencyProvider = new(provider);

        Accumulator accumulator = provider.GetRequiredService<Accumulator>();
        var sender = provider.GetRequiredService<IStreamRunner>();

        Fin<IAsyncEnumerable<Response>> result = sender.Run(new Request(), HandlerRuntime.New(dependencyProvider, EnvIO.New()));

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