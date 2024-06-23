using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crud.Domain.Events;
using Crud.Domain.Repositories;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Crud.Core.Events.Mutated;

public sealed class QuestionMutatedDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder.AddHandler<Handler>();
    }
}

internal sealed class Handler(IQuestionRepository repository, ILogger<Handler> logger) : IHandler<QuestionMutatedEvent>
{
    readonly IQuestionRepository _repository = repository;
    readonly ILogger<Handler> _logger = logger;

    public Aff<Unit> Define(QuestionMutatedEvent request, CancellationToken cancellationToken = default) =>
        from question in _repository.ReadAsync(request.Id, cancellationToken)
        from _ in Eff(() =>
        {
            _logger.LogInformation("Se ha realizado un cambio en la tabla Questions, cambio de tipo: {State}, valores actuales: {Entity}",
                request.CurrentState.ToString(), question);

            return unit;
        })
        select unit;

}
