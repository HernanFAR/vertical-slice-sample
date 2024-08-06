using Crud.CrossCutting.Pipelines;
using Crud.Domain.Rules.DataAccess;
using Crud.Domain.Rules.Services;
using Crud.Domain.ValueObjects;
using VSlices.Base.Builder;
using VSlices.Base.Core;

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

internal sealed class RequestHandler : IHandler<Command>
{
    public Eff<VSlicesRuntime, Unit> Define(Command input) =>
        from repository in provide<IQuestionRepository>()
        from manager in provide<QuestionManager>()
        from optionalQuestion in repository.GetOrOption(input.Id)
        from question in optionalQuestion.ToEff(ExtensibleExpected.NotFound("No se ha encontrado la pregunta", []))
        from _ in manager.Delete(question)
        select unit;

}
