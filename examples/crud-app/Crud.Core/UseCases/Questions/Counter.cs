using Cronos;
using Crud.CrossCutting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VSlices.Core.RecurringJob;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Questions.Counter;

public sealed class CounterFeatureDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder) =>
        featureBuilder.AddRecurringJob<RecurringJob>()
                      .AddHandler<Handler>();
}

internal sealed class RecurringJob(IRequestRunner runner) : IRecurringJobDefinition
{
    private readonly IRequestRunner _runner = runner;
    public string Identifier => "CounterRecurringJob";

    public CronExpression Cron => CronExpression.EveryMinute;

    public ValueTask ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _runner.Run(Query.Instance, cancellationToken);

        return ValueTask.CompletedTask;
    }
}

internal sealed record Query : IRequest
{
    public static Query Instance { get; } = new();
}

internal sealed class Handler : IHandler<Query>
{
    public Eff<VSlicesRuntime, Unit> Define(Query request) =>
        from context in provide<AppDbContext>()
        from logger in provide<ILogger<Handler>>()
        from cancelToken in cancelToken
        from count in liftEff(() => context.Questions.CountAsync(cancelToken))
        from _ in liftEff(() => logger.LogInformation("Total questions: {Count}.", count))
        select unit;
}
