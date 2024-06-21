using Crud.Domain;
using Crud.Domain.Repositories;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Exists;

public sealed class ExistsQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder.AddEndpoint<EndpointDefinition>()
            .AddHandler<Handler>();
    }
}

internal sealed record Query(QuestionId Id) : IRequest;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions/{id:Guid}";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapMethods(Path, new[] { HttpMethods.Head }, HandlerAsync)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("ExistsQuestion");
    }

    public async Task<IResult> HandlerAsync(
        [FromRoute]
        Guid id,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Query query = new(new QuestionId(id));

        return await runner
            .RunAsync(query, cancellationToken)
            .MatchResult(_ => TypedResults.NoContent());
    }
}

internal sealed class Handler(IQuestionRepository repository) : IHandler<Query>
{
    private readonly IQuestionRepository _repository = repository;

    public Aff<Unit> Define(Query request, CancellationToken cancellationToken = default) =>
        from exists in AffMaybe(async () => await _repository.ExistsAsync(request.Id, cancellationToken))
        from _ in guard(exists, new NotFound("No se ha encontrado la pregunta").AsError)
        select unit;

}
