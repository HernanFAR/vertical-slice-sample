using Crud.CrossCutting.Pipelines;
using Crud.Domain;
using Crud.Domain.Repositories;
using Crud.Domain.Services;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Delete;

public sealed class DeleteQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddEndpoint<EndpointDefinition>()
            .AddExceptionHandlingPipeline<LoggingExceptionHandlerPipeline<Command, Unit>>()
            .AddHandler<Handler>();
    }
}

internal sealed record Command(QuestionId Id) : IRequest<Unit>;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions/{id:Guid}";

    public void Define(IEndpointRouteBuilder builder)
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

internal sealed class Handler(
    IQuestionRepository repository,
    QuestionManager manage) 
    : IHandler<Command>
{
    readonly IQuestionRepository _repository = repository;
    readonly QuestionManager _manage = manage;

    public Aff<Unit> Define(Command request, CancellationToken cancellationToken) =>
        from question in _repository.ReadAsync(request.Id, cancellationToken)
        from _ in _manage.DeleteAsync(question, cancellationToken)
        select unit;

}
