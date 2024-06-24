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
        featureBuilder.AddHandler<Handler>();
    }
}

internal sealed class Handler(IQuestionRepository repository, ILogger<Handler> logger)
    : IHandler<QuestionMutatedEvent>
{
    private readonly IQuestionRepository _repository = repository;
    private readonly ILogger<Handler> _logger = logger;

    public Aff<Runtime, Unit> Define(QuestionMutatedEvent request) =>
        from cancelToken in cancelToken<Runtime>()
        from question in _repository.ReadAsync(request.Id, cancelToken)
        from _ in Eff(() =>
        {
            _logger.LogInformation("Se ha realizado un cambio en la tabla Questions, cambio de tipo: {State}, valores actuales: {Entity}",
                request.CurrentState.ToString(), question);

            return unit;
        })
        select unit;
}
