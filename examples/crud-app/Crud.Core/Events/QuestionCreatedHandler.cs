using Crud.CrossCutting.Pipelines;
using Crud.Domain.Events;
using Crud.Domain.Repositories;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;
using VSlices.CrossCutting.Pipeline.EventFiltering;

// ReSharper disable once CheckNamespace
namespace Crud.Core.Events.Created;

public sealed class QuestionCreatedDependencies : IFeatureDependencies
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
    readonly IQuestionRepository _repository = repository;
    readonly ILogger<Handler> _logger = logger;

    public Aff<Runtime, Unit> Define(QuestionMutatedEvent request) =>
        from question in _repository.Read(request.Id)
            .Bind(question => Eff(() =>
            {
                _logger.LogInformation(
                    "Se ha creado una nueva pregunta usando los siguientes valores: {Entity}", question);

                return unit;
            }))
        select unit;
}

internal sealed class EventFilter : IEventFilter<QuestionMutatedEvent, Handler>
{
    public Aff<Runtime, bool> Define(QuestionMutatedEvent @event) => 
        @event.CurrentState == EState.Created ? trueAff : falseAff;
}