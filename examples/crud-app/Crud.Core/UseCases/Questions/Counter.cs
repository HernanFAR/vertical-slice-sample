using Cronos;
using Crud.CrossCutting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Core.RecurringJob;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Questions.Counter;

public sealed record Query : IRequest
{
    public static Query Instance { get; } = new();
}

public sealed class CounterFeatureDependencies : IFeatureDependencies<Query>
{
    public static void DefineDependencies(IFeatureStartBuilder<Query, Unit> feature) =>
        feature.FromIntegration.Using<RecurringJobIntegrator>()
               .Execute<RequestHandler>()
               .WithBehaviorChain(chain => chain
                                      .AddLogging().InSpanish()
                                      .AddLoggingException().InSpanish());
}

internal sealed class RecurringJobIntegrator(IRequestRunner runner) : IRecurringJobIntegrator
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

internal sealed class RequestHandler : IHandler<Query>
{
    public Eff<VSlicesRuntime, Unit> Define(Query input) =>
        from context in provide<AppDbContext>()
        from logger in provide<ILogger<RequestHandler>>()
        from timeProvider in provide<TimeProvider>()
        from cancelToken in cancelToken
        from count in liftEff(() => context.Questions.CountAsync(cancelToken))
        from _ in liftEff(() => logger.LogInformation("Total questions: {Count} at: {CurrentTime}.", 
                                                      count, 
                                                      timeProvider.GetUtcNow().UtcDateTime))
        select unit;
}
