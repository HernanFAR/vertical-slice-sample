using Crud.CrossCutting.Pipelines;
using Crud.Domain;
using Crud.Domain.Repositories;
using Crud.Domain.Services;
using Crud.Domain.ValueObjects;
using LanguageExt.SysX.Live;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Delete;

public sealed class DeleteQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddEndpoint<EndpointDefinition>()
            .AddLoggingFor<Command>().UsingSpanishTemplate()
            .AddExceptionHandling<LoggingExceptionHandlerPipeline<Command, Unit>>()
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

    public Aff<Runtime, Unit> Define(Command request) =>
        from question in _repository.Read(request.Id)
        from _ in _manage.Delete(question)
        select unit;

}
