using Crud.CrossCutting.Pipelines;
using Crud.Domain.Rules.DataAccess;
using Crud.Domain.Rules.Services;
using Crud.Domain.ValueObjects;
using VSlices.Base.Builder;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Questions.Delete;

public sealed record Command(QuestionId Id) : IRequest<Unit>;

public sealed class DeleteQuestionDependencies : IFeatureDependencies<Command>
{
    public static void DefineDependencies(IFeatureStartBuilder<Command, Unit> feature) =>
        feature.FromIntegration.With<EndpointDefinition>()
               .Executing<RequestHandler>()
               .AddBehaviors(chain => chain
                                      .AddLogging().UsingSpanish()
                                      .AddLoggingException().UsingSpanish());
}

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions/{id:Guid}";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapDelete(Path, Handler)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("DeleteQuestion")
            .WithTags("Questions");
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
            .MatchResult(TypedResults.NoContent());
    }
}

internal sealed class RequestHandler : IRequestHandler<Command>
{
    public Eff<VSlicesRuntime, Unit> Define(Command request) =>
        from repository in provide<IQuestionRepository>()
        from manager in provide<QuestionManager>()
        from optionalQuestion in repository.GetOrOption(request.Id)
        from question in optionalQuestion.ToEff(ExtensibleExpected.NotFound("No se ha encontrado la pregunta", []))
        from _ in manager.Delete(question)
        select unit;

}
