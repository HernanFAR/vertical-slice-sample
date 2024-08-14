using Cronos;
using FluentAssertions;
using Hangfire;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
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

    public class RequestHandler : IHandler<Query>
    {
        public Eff<VSlicesRuntime, Unit> Define(Query request) =>
            from accumulator in provide<Accumulator>()
            from _ in liftEff(accumulator.Increment)
            select unit;
    }

    public class RecurringJobListener(IRequestRunner runner) : IRecurringJobIntegrator
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
        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddReflectionRequestRunner()
                       .AddHostedTaskListener()
                       .AddLogging()
                       .AddRecurringJobListener()
                       .AddSingleton<Accumulator>();

        new FeatureDefinition<Query, Unit>(services).FromIntegration.Using<RecurringJobListener>()
                                                    .Execute<RequestHandler>();

        var provider = services.BuildServiceProvider();

        var backgroundTaskListener = provider.GetRequiredService<IBackgroundTaskListener>();
        var accumulator = provider.GetRequiredService<Accumulator>();

        _ = backgroundTaskListener.ExecuteRegisteredJobs(default);

        accumulator.Count.Should().Be(0);

        await Task.Delay(7500);

        accumulator.Count.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task StartRecurringJob_Hangfire()
    {
        MsSqlContainer container = new MsSqlBuilder()
                                    .WithPassword("Hangfire.2024IB")
                                    .Build();

        await container.StartAsync();

        var services = new ServiceCollection()
                       .AddVSlicesRuntime()
                       .AddReflectionRequestRunner()
                       .AddHangfireTaskListener(config => config.UseSqlServerStorage(container.GetConnectionString()))
                       .AddLogging()
                       .AddRecurringJobListener()
                       .AddSingleton<Accumulator>();

        new FeatureDefinition<Query, Unit>(services).FromIntegration.Using<RecurringJobListener>()
                                                    .Execute<RequestHandler>();

        var provider = services.BuildServiceProvider();

        var backgroundEventListener = provider.GetServices<IHostedService>();
        var accumulator             = provider.GetRequiredService<Accumulator>();

        foreach (IHostedService service in backgroundEventListener)
        {
            _ = service.StartAsync(default);
        }
        
        accumulator.Count.Should().Be(0);

        await Task.Delay(7500);

        accumulator.Count.Should().BeGreaterOrEqualTo(2);

        await container.StopAsync();
    }
}
