using Cronos;
using Crud.CrossCutting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Base.Definitions;
using VSlices.Core.Integration.RecurringJob;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Questions.Counter;

public sealed record Query : IInput
{
    public static Query Instance { get; } = new();
}

public sealed class CounterFeatureDefinition : IFeatureDefinition
{
    public static Unit Define(FeatureComposer feature) =>
        feature.With<Query>().ExpectNoOutput()
               .ByExecuting<Behavior>(chain => chain.AddLogging().InSpanish()
                                                           .AddLoggingException().InSpanish())
               .AndBindTo<RecurringJobIntegrator>();
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

internal sealed class Behavior : IBehavior<Query>
{
    public Eff<VSlicesRuntime, Unit> Define(Query input) =>
        from context in provide<AppDbContext>()
        from logger in provide<ILogger<Behavior>>()
        from timeProvider in provide<TimeProvider>()
        from cancelToken in cancelToken
        from _ in liftEff(async () =>
        {
            int count = await context.Questions.CountAsync(cancelToken);

            logger.LogInformation("Total questions: {Count} at: {CurrentTime}.",
                                  count, 
                                  timeProvider.GetUtcNow().UtcDateTime);

        })
        select unit;
}
