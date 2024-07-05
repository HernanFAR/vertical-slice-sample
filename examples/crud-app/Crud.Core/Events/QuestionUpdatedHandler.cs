using Crud.CrossCutting.Pipelines;
using Crud.Domain.Events;
using Crud.Domain.Repositories;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;
using VSlices.CrossCutting.Pipeline.EventFiltering;

// ReSharper disable once CheckNamespace
namespace Crud.Core.Events.Updated;

public sealed class QuestionUpdatedDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddHandler<Handler>()
            .AddEventFilteringUsing<EventFilter>().UsingSpanishTemplate()
            .AddExceptionHandling<LoggingExceptionHandlerPipeline<QuestionMutatedEvent, Unit>>();
    }
}

internal sealed class Handler(IQuestionRepository repository, ILogger<Handler> logger)
    : IHandler<QuestionMutatedEvent>
{
    private readonly IQuestionRepository _repository = repository;
    private readonly ILogger<Handler> _logger = logger;

    public Aff<Runtime, Unit> Define(QuestionMutatedEvent request)
    {
        return from question in _repository.Read(request.Id)
            .Bind(question => Eff(() =>
            {
                _logger.LogInformation(
                    "Se ha actualizado una pregunta, ahora tiene los siguientes valores: {Entity}", question);

                return unit;
            }))
               select unit;
    }
}

internal sealed class EventFilter : IEventFilter<QuestionMutatedEvent, Handler>
{
    public Aff<Runtime, bool> Define(QuestionMutatedEvent @event)
    {
        return @event.CurrentState == EState.Updated ? trueAff : falseAff;
    }
}