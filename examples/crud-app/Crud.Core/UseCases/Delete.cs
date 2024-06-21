using Crud.Domain;
using Crud.Domain.Repositories;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Delete;

public sealed class DeleteQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder.AddEndpoint<EndpointDefinition>()
            .AddHandler<Handler>();
    }
}

internal sealed record Command(QuestionId Id) : IRequest<Unit>;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions/{id:Guid}";

    public void DefineEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete(Path, HandlerAsync)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("DeleteQuestion");
    }

    public async Task<IResult> HandlerAsync(
        [FromRoute]
        Guid id,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Command command = new(new QuestionId(id));

        return await runner
            .RunAsync(command, cancellationToken)
            .MatchResult(TypedResults.Ok);
    }
}

internal sealed class Handler(IQuestionRepository repository) : IHandler<Command>
{
    private readonly IQuestionRepository _repository = repository;

    public Aff<Unit> Define(Command request, CancellationToken cancellationToken) =>
        from question in AffMaybe(async () => await _repository.ReadAsync(request.Id, cancellationToken))
        from _ in AffMaybe(async () => await _repository.DeleteAsync(question, cancellationToken))
        select unit;

}
