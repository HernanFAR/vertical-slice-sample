using Crud.CrossCutting.Pipelines;
using Crud.Domain.Rules.DataAccess;
using Crud.Domain.ValueObjects;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Exists;

public sealed class ExistsQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddEndpoint<EndpointDefinition>()
            .AddLoggingBehaviorFor<Query>()
                .UsingSpanishTemplate()
            .AddExceptionHandlingBehavior<LoggingExceptionHandlerPipeline<Query, Unit>>()
            .AddHandler<Handler>();
    }
}

internal sealed record Query(QuestionId Id) : IRequest;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions/{id:Guid}";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapMethods(Path, new[] { HttpMethods.Head }, Handler)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("ExistsQuestion");
    }

    public IResult Handler(
        [FromRoute]
        Guid id,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Query query = new(new QuestionId(id));

        return runner
            .Run(query, cancellationToken)
            .MatchResult(TypedResults.NoContent());
    }
}

internal sealed class Handler : IHandler<Query>
{
    public Eff<VSlicesRuntime, Unit> Define(Query request) =>
        from repository in provide<IQuestionRepository>()
        from exists in repository.Exists(request.Id)
        from _ in guard(exists, new NotFound("No se ha encontrado la pregunta").AsError)
        select unit;

}
