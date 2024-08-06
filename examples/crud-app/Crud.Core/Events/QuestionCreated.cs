using Crud.CrossCutting.Pipelines;
using Crud.Domain.Rules.DataAccess;
using Crud.Domain.Rules.Events;
using Microsoft.Extensions.Logging;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Core.Events;
using VSlices.CrossCutting.Pipeline.Filtering;

// ReSharper disable once CheckNamespace
namespace Crud.Core.Events.QuestionCreated;

public sealed class QuestionCreatedDependencies : IFeatureDependencies<QuestionMutatedEvent>
{
    public static void DefineDependencies(IFeatureStartBuilder<QuestionMutatedEvent, Unit> define) =>
        define.ByExecuting<RequestHandler>()
              .AddBehaviors(chain => chain
                                     .AddFilteringUsing<Filter>().UsingEnglish()
                                     .AddLogging().UsingEnglish()
                                     .AddLoggingException().UsingEnglish());
}

internal sealed class RequestHandler : IHandler<QuestionMutatedEvent>
{
    public Eff<VSlicesRuntime, Unit> Define(QuestionMutatedEvent input) =>
        from repository in provide<IQuestionRepository>()
        from logger in provide<ILogger<QuestionMutatedEvent>>()
        from question in repository.Get(input.Id)
        from _ in liftEff(env =>
        {
            logger.LogInformation("Se ha creado un nuevo recurso en la tabla Questions, " +
                                  "con valores: {Entity}", question);

            return unit;
        })
        select unit;
}

internal sealed class Filter : IEventFilter<QuestionMutatedEvent, RequestHandler>
{
    public Eff<VSlicesRuntime, bool> DefineFilter(QuestionMutatedEvent feature) =>
        from shouldProcess in liftEff(() => feature.CurrentState == EState.Created)
        select shouldProcess;
}