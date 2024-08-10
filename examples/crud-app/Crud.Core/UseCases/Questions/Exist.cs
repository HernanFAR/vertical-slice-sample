using Crud.Core.UseCases.Questions.Counter;
using Crud.CrossCutting.Pipelines;
using Crud.Domain.Rules.DataAccess;
using Crud.Domain.ValueObjects;
using VSlices.Base.Builder;
using VSlices.Base.Core;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Questions.Exists;

public sealed record Query(QuestionId Id) : IRequest;

public sealed class ExistsQuestionDependencies : IFeatureDependencies<Query>
{
    public static void DefineDependencies(IFeatureStartBuilder<Query, Unit> feature) =>
        feature.FromIntegration.Using<RecurringJobDefinition>()
               .Execute<Handler>()
               .WithBehaviorChain(chain => chain
                                      .AddLogging().UsingSpanish()
                                      .AddLoggingException().UsingSpanish());
}


internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions/{id:Guid}";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapMethods(Path, new[] { HttpMethods.Head }, Handler)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("ExistsQuestion")
            .WithTags("Questions");
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
    public Eff<VSlicesRuntime, Unit> Define(Query input) =>
        from repository in provide<IQuestionRepository>()
        from exists in repository.Exists(input.Id)
        from _ in guard(exists, notFound("No se ha encontrado la pregunta"))
        select unit;

}
