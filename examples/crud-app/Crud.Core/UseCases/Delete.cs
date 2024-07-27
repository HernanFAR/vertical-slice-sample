using Crud.CrossCutting.Pipelines;
using Crud.Domain.Repositories;
using Crud.Domain.Services;
using Crud.Domain.ValueObjects;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Delete;

public sealed class DeleteQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddEndpoint<EndpointDefinition>()
            .AddLoggingBehaviorFor<Command>()
                .UsingSpanishTemplate()
            .AddExceptionHandlingBehavior<LoggingExceptionHandlerPipeline<Command, Unit>>()
            .AddHandler<Handler>();
    }
}

internal sealed record Command(QuestionId Id) : IRequest<Unit>;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions/{id:Guid}";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapDelete(Path, Handler)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("DeleteQuestion");
    }

    public IResult Handler(
        [FromRoute]
        Guid id,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Command command = new(new QuestionId(id));

        return runner
            .Run(command, cancellationToken)
            .MatchResult(TypedResults.Ok);
    }
}

internal sealed class Handler : IHandler<Command>
{
    public Eff<HandlerRuntime, Unit> Define(Command request) =>
        from repository in provide<IQuestionRepository>()
        from manager in provide<QuestionManager>()
        from question in repository.Read(request.Id)
        from _ in manager.Delete(question)
        select unit;

}
