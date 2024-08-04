using Cronos;
using FluentAssertions;
using Hangfire;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;
using VSlices.Base;
using VSlices.Core.UseCases;
using VSlices.CrossCutting.BackgroundTaskListener;
using static LanguageExt.Prelude;
using static VSlices.VSlicesPrelude;

namespace VSlices.Core.RecurringJob.IntegTests;

public class RecurringJobTests
{
    public class Accumulator
    {
        public int Count;
        public void Increment() => Count++;
        public void Reset() => Count = 0;    
    }

    public record Query : IRequest;

    public class Handler : IHandler<Query>
    {
        public Eff<VSlicesRuntime, Unit> Define(Query request) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(() =>
            {
                accumulator.Increment();
            })
            select unit;
    }

    public class RecurringJobListener(IRequestRunner runner) : IRecurringJobDefinition
    {
        private readonly IRequestRunner _runner = runner;

        public string Identifier => nameof(RecurringJobListener);

        public CronExpression Cron => CronExpression.EverySecond;

        public ValueTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _runner.Run(new Query());

            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task StartRecurringJob_AspNetCoreHosting()
    {
        var provider = new ServiceCollection()
                       // Runtime
                       .AddVSlicesRuntime()
                       .AddReflectionRequestRunner()
                       .AddHostedTaskListener()
                       // Extras
                       .AddLogging()
                       // Testing
                       .AddSingleton<Accumulator>()
                       .AddTransient<IHandler<Query, Unit>, Handler>()
                       .AddSingleton<IRecurringJobDefinition, RecurringJobListener>()
                       .AddRecurringJobListener()
                       .BuildServiceProvider();

        var backgroundTaskListener = provider.GetRequiredService<IBackgroundTaskListener>();
        var accumulator = provider.GetRequiredService<Accumulator>();

        _ = backgroundTaskListener.ExecuteRegisteredJobs(default);

        accumulator.Count.Should().Be(0);

        await Task.Delay(5000);

        accumulator.Count.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task StartRecurringJob_Hangfire()
    {
        MsSqlContainer container = new MsSqlBuilder()
                                    .WithPassword("Hangfire.2024IB")
                                    .Build();

        await container.StartAsync();

        ServiceProvider provider = new ServiceCollection()
                                   // Runtime
                                   .AddVSlicesRuntime()
                                   .AddReflectionRequestRunner()
                                   .AddHangfireTaskListener(config => config.UseSqlServerStorage(container.GetConnectionString()))
                                   // Extras
                                   .AddLogging()
                                   // Testing
                                   .AddSingleton<Accumulator>()
                                   .AddTransient<IHandler<Query, Unit>, Handler>()
                                   .AddSingleton<IRecurringJobDefinition, RecurringJobListener>()
                                   .AddRecurringJobListener()
                                   .BuildServiceProvider();

        var backgroundEventListener = provider.GetServices<IHostedService>();
        var accumulator             = provider.GetRequiredService<Accumulator>();

        foreach (IHostedService service in backgroundEventListener)
        {
            _ = service.StartAsync(default);
        }
        
        accumulator.Count.Should().Be(0);

        await Task.Delay(5000);

        accumulator.Count.Should().BeGreaterOrEqualTo(2);

        await container.StopAsync();
    }
}
