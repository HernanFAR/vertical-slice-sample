using Crud.CrossCutting.Pipelines;
using Crud.Domain.Rules.DataAccess;
using Crud.Domain.Rules.Events;
using Microsoft.Extensions.Logging;
using VSlices.Base.Builder;
using VSlices.Core.Events;

// ReSharper disable once CheckNamespace
namespace Crud.Core.Events.Mutated;

public sealed class QuestionMutatedDependencies : IFeatureDependencies<QuestionMutatedEvent>
{
    public static void DefineDependencies(IFeatureStartBuilder<QuestionMutatedEvent, Unit> defineFeature) =>
        defineFeature.Execute<RequestHandler>()
                     .AddBehaviors(chain => chain
                                            .AddLogging().UsingEnglish()
                                            .AddLoggingException().UsingSpanish());
}

internal sealed class RequestHandler : IEventHandler<QuestionMutatedEvent>
{
    public Eff<VSlicesRuntime, Unit> Define(QuestionMutatedEvent request) =>
        from repository in provide<IQuestionRepository>()
        from logger in provide<ILogger<QuestionMutatedEvent>>()
        from optionalQuestion in repository.GetOrOption(request.Id)
        from _ in liftEff(() => optionalQuestion.BiIter(
            Some: question =>
            {
                logger.LogInformation("Se ha realizado un cambio en la tabla Questions, cambio de " +
                                      "tipo: {State}, valores actuales: {Entity}",
                                      request.CurrentState.ToString(),
                                      question);
            },
            None: _ =>
            {
                logger.LogInformation("Se ha realizado un cambio en la tabla Questions, cambio de " +
                                      "tipo: {State}, no se ha encontrado la entidad",
                                      request.CurrentState.ToString());
            }))
        select unit;
}
