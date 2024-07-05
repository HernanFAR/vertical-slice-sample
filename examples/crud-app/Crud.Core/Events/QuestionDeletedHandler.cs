using Crud.CrossCutting.Pipelines;
using Crud.Domain.Events;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;
using VSlices.CrossCutting.Pipeline.EventFiltering;

// ReSharper disable once CheckNamespace
namespace Crud.Core.Events.Deleted;

public sealed class QuestionDeletedDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddHandler<Handler>()
            .AddEventFilteringUsing<EventFilter>().UsingSpanishTemplate()
            .AddExceptionHandling<LoggingExceptionHandlerPipeline<QuestionMutatedEvent, Unit>>();
    }
}

internal sealed class Handler(ILogger<Handler> logger)
    : IHandler<QuestionMutatedEvent>
{
    private readonly ILogger<Handler> _logger = logger;

    public Aff<Runtime, Unit> Define(QuestionMutatedEvent request)
    {
        return from _ in Eff(() =>
            {
                _logger.LogInformation("Se ha borrado un registro en la tabla Questions, id {QuestionId}.", request.Id);

                return unit;
            })
               select unit;
    }
}

internal sealed class EventFilter : IEventFilter<QuestionMutatedEvent, Handler>
{
    public Aff<Runtime, bool> Define(QuestionMutatedEvent @event)
    {
        return @event.CurrentState == EState.Removed ? trueAff : falseAff;
    }
}