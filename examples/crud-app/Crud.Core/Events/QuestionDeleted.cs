using Crud.CrossCutting.Pipelines;
using Crud.Domain.Rules.DataAccess;
using Crud.Domain.Rules.Events;
using Microsoft.Extensions.Logging;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Core.Events;
using VSlices.CrossCutting.Pipeline.Filtering;

// ReSharper disable once CheckNamespace
namespace Crud.Core.Events.Mutated;

public sealed class QuestionMutatedDependencies : IFeatureDependencies<QuestionMutatedEvent>
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
        from logger in provide<ILogger<QuestionMutatedEvent>>()
        from _ in liftEff(_ =>
        {
            logger.LogInformation("Se ha eliminado un recurso en la tabla Questions, Id: " +
                                  "{Id}",  input.Id);

            return unit;
        })
        select unit;
}

internal sealed class Filter : IEventFilter<QuestionMutatedEvent, RequestHandler>
{
    public Eff<VSlicesRuntime, bool> DefineFilter(QuestionMutatedEvent feature) =>
        from shouldProcess in liftEff(() => feature.CurrentState == EState.Removed)
        select shouldProcess;
}
