using Crud.CrossCutting.Pipelines;
using Crud.Domain;
using Crud.Domain.Events;
using Crud.Domain.Repositories;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Crud.Core.Events.Mutated;

public sealed class QuestionMutatedDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddHandler<Handler>()
            .AddExceptionHandlingBehavior<LoggingExceptionHandlerPipeline<QuestionMutatedEvent, Unit>>();
    }
}

internal sealed class Handler(IQuestionRepository repository, ILogger<Handler> logger)
    : IHandler<QuestionMutatedEvent>
{
    readonly IQuestionRepository _repository = repository;
    readonly ILogger<Handler> _logger = logger;

    public Aff<Runtime, Unit> Define(QuestionMutatedEvent request) =>
        from question in _repository.Read(request.Id)
            .BiBind(
                Succ: question => Eff(() =>
                {
                    _logger.LogInformation(
                        "Se ha realizado un cambio en la tabla Questions, cambio de tipo: {State}, valores actuales: {Entity}",
                        request.CurrentState.ToString(), question);

                    return unit;
                }),
                Fail: _ => Eff(() =>
                {
                    _logger.LogInformation(
                        "Se ha realizado un cambio en la tabla Questions, cambio de tipo: {State}, no se ha encontrado la entidad",
                        request.CurrentState.ToString());

                    return unit;
                }))
        select unit;
}
