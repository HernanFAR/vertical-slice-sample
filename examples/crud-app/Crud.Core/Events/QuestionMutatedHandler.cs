﻿using Crud.CrossCutting.Pipelines;
using Crud.Domain.Events;
using Crud.Domain.Repositories;
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

internal sealed class Handler
    : IHandler<QuestionMutatedEvent>
{
    public Eff<HandlerRuntime, Unit> Define(QuestionMutatedEvent request) =>
        from repository in provide<IQuestionRepository>()
        from logger in provide<ILogger>()
        from question in repository
            .Read(request.Id)
            .Match(Succ: question => liftEff(() => 
            {
                logger.LogInformation("Se ha realizado un cambio en la tabla Questions, cambio de tipo: {State}, valores actuales: {Entity}", 
                                      request.CurrentState.ToString(), 
                                      question);

                return unit;
            }),
            Fail: _ => liftEff(() => 
           {
               logger.LogInformation("Se ha realizado un cambio en la tabla Questions, cambio de " +
                                     "tipo: {State}, no se ha encontrado la entidad",
                                     request.CurrentState.ToString());

               return unit;
           }))
        select unit;
}
