using Crud.Domain.Rules.Events;
using Microsoft.Extensions.Logging;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Base.Definitions;
using VSlices.CrossCutting.Interceptor.Filtering;

// ReSharper disable once CheckNamespace
namespace Crud.Core.Events.Mutated;

public sealed class QuestionMutatedDefinition : IFeatureDefinition
{
    public static Unit Define(FeatureComposer starting) =>
        starting.With<QuestionMutatedEvent>()
                .ExpectNoOutput()
                .ByExecuting<RequestBehavior>(chain => chain.AddFilteringUsing<Filter>().InSpanish()
                                                            .AddLogging().InSpanish()
                                                            .AddLoggingException().InSpanish())
                .AndNoBind();
}

internal sealed class Filter : IEventFilter<QuestionMutatedEvent, RequestBehavior>
{
    public Eff<VSlicesRuntime, bool> DefineFilter(QuestionMutatedEvent feature) =>
        from shouldProcess in liftEff(() => feature.CurrentState == EState.Removed)
        select shouldProcess;
}

internal sealed class RequestBehavior : IBehavior<QuestionMutatedEvent>
{
    public Eff<VSlicesRuntime, Unit> Define(QuestionMutatedEvent input) =>
        from logger in provide<ILogger<QuestionMutatedEvent>>()
        from _ in liftEff(_ =>
        {
            logger.LogInformation("Se ha eliminado un recurso en la tabla Questions, Id: " +
                                  "{Id}", input.Id);

            return unit;
        })
        select unit;
}
