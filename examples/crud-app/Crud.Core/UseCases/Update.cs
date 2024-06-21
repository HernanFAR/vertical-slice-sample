using Crud.Domain;
using Crud.Domain.Repositories;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Update;

public sealed class UpdateQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder.AddEndpoint<EndpointDefinition>()
            .AddHandler<Handler>();
    }
}

public sealed record UpdateQuestionContract(string Text);

internal sealed record Command(QuestionId Id, string Text) : IRequest<Unit>;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions/{id:Guid}";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapPut(Path,  HandlerAsync)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .WithName("UpdateQuestion");
    }

    public async Task<IResult> HandlerAsync(
        [FromBody]
        UpdateQuestionContract contract,
        [FromRoute]
        Guid id,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Command command = new(new QuestionId(id), contract.Text);

        return await runner
            .RunAsync(command, cancellationToken)
            .MatchResult(TypedResults.Ok);
    }
}

internal sealed class Handler(IQuestionRepository repository) : IHandler<Command>
{
    private readonly IQuestionRepository _repository = repository;

    public Aff<Unit> Define(Command request, CancellationToken cancellationToken = default) =>
        from exists in AffMaybe(async () => await _repository.ExistsAsync(request.Id, cancellationToken))
        from question in exists
            ? AffMaybe(async () => await _repository.ReadAsync(request.Id, cancellationToken))
            : SuccessAff(new Question(request.Id, string.Empty))
        from _1 in Eff(() => question.UpdateState(request.Text))    
        from _2 in AffMaybe(async () => await _repository.UpdateAsync(question, cancellationToken))
        select unit;

}
