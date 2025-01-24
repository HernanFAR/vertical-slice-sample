using Crud.Domain.Rules.DataAccess;
using Crud.Domain.Rules.Events;
using Microsoft.Extensions.Logging;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Base.Definitions;
using VSlices.CrossCutting.Interceptor.Filtering;

// ReSharper disable once CheckNamespace
namespace Crud.Core.Events.QuestionUpdated;

public sealed class QuestionMutatedDefinition : IFeatureDefinition
{
    public static Unit Define(FeatureComposer starting) =>
        starting.With<QuestionMutatedEvent>().ExpectNoOutput()
                .ByExecuting<RequestBehavior>(chain => chain.AddFilteringUsing<Filter>().InSpanish()
                                                            .AddLogging().InSpanish()
                                                            .AddLoggingException().InSpanish())
                .AndNoBind();
}

internal sealed class Filter : IEventFilter<QuestionMutatedEvent, RequestBehavior>
{
    public Eff<VSlicesRuntime, bool> DefineFilter(QuestionMutatedEvent feature) =>
        from shouldProcess in liftEff(() => feature.CurrentState == EState.Updated)
        select shouldProcess;
}

internal sealed class RequestBehavior : IBehavior<QuestionMutatedEvent>
{
    public Eff<VSlicesRuntime, Unit> Define(QuestionMutatedEvent input) =>
        from repository in provide<IQuestionRepository>()
        from logger in provide<ILogger<QuestionMutatedEvent>>()
        from question in repository.Get(input.Id)
        from _ in liftEff(env =>
        {
            logger.LogInformation("Se ha actualizado un recurso en la tabla Questions, " +
                                  "a los valores: {Entity}",
                                  question);

            return unit;
        })
        select unit;
}
