using Crud.Domain.Rules.DataAccess;
using Crud.Domain.Rules.Services;
using Crud.Domain.ValueObjects;
using VSlices.Base.Core;
using VSlices.Base.Definitions;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Questions.Delete;

public sealed record Command(QuestionId Id) : IInput<Unit>;

public sealed class DeleteQuestionDefinition : IFeatureDefinition
{
    public static Unit Define(FeatureComposer feature) =>
        feature.With<Command>().ExpectNoOutput()
               .ByExecuting<RequestBehavior>(chain => chain.AddLogging().InSpanish()
                                                           .AddLoggingException().InSpanish())
               .AndBindTo<EndpointIntegrator>();
}

internal sealed class EndpointIntegrator : IEndpointIntegrator
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

internal sealed class RequestBehavior : IBehavior<Command>
{
    public Eff<VSlicesRuntime, Unit> Define(Command input) =>
        from repository in provide<IQuestionRepository>()
        from manager in provide<QuestionManager>()
        from optionalQuestion in repository.GetOrOption(input.Id)
        from question in optionalQuestion.ToEff(ExtensibleExpected.NotFound("No se ha encontrado la pregunta", []))
        from _ in manager.Delete(question)
        select unit;

}
