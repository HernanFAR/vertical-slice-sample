using Crud.CrossCutting.Pipelines;
using Crud.Domain.Rules.DataAccess;
using Crud.Domain.Rules.Events;
using Microsoft.Extensions.Logging;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Core.Events;

// ReSharper disable once CheckNamespace
namespace Crud.Core.Events.Mutated;

public sealed class QuestionMutatedDependencies : IFeatureDependencies<QuestionMutatedEvent>
{
    public static void DefineDependencies(IFeatureStartBuilder<QuestionMutatedEvent, Unit> define) =>
        define.ByExecuting<RequestHandler>()
              .AddBehaviors(chain => chain
                                     .AddLogging().UsingEnglish()
                                     .AddLoggingException().UsingSpanish());
}

internal sealed class RequestHandler : IHandler<QuestionMutatedEvent>
{
    public Eff<VSlicesRuntime, Unit> Define(QuestionMutatedEvent input) =>
        from repository in provide<IQuestionRepository>()
        from logger in provide<ILogger<QuestionMutatedEvent>>()
        from optionalQuestion in repository.GetOrOption(input.Id)
        from _ in liftEff(() => optionalQuestion.BiIter(
            Some: question =>
            {
                logger.LogInformation("Se ha realizado un cambio en la tabla Questions, cambio de " +
                                      "tipo: {State}, valores actuales: {Entity}",
                                      input.CurrentState.ToString(),
                                      question);
            },
            None: _ =>
            {
                logger.LogInformation("Se ha realizado un cambio en la tabla Questions, cambio de " +
                                      "tipo: {State}, no se ha encontrado la entidad",
                                      input.CurrentState.ToString());
            }))
        select unit;
}
