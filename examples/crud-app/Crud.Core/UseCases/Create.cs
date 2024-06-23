using Crud.Domain;
using Crud.Domain.Repositories;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Create;

public sealed class CreateQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder.AddEndpoint<EndpointDefinition>()
            .AddHandler<Handler>();
    }
}

public sealed record CreateQuestionContract(string Text);

internal sealed record Command(string Text) : IRequest<Unit>;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapPost(Path, HandlerAsync)
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .WithName("CreateQuestion");
    }

    public async Task<IResult> HandlerAsync(
        [FromBody]
        CreateQuestionContract contract,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Command command = new(contract.Text);

        return await runner
            .RunAsync(command, cancellationToken)
            .MatchResult(_ => TypedResults.Created());
    }
}

internal sealed class Handler(IQuestionRepository repository) : IHandler<Command, Unit>
{
    private readonly IQuestionRepository _repository = repository;

    public Aff<Unit> Define(Command request, CancellationToken cancellationToken = default) =>
        from question in SuccessEff(Question.Create(request.Text))
        from _ in _repository.CreateAsync(question, cancellationToken)
        select unit;
}
