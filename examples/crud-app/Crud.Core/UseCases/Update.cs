using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Extensions;
using Crud.CrossCutting.Pipelines;
using Crud.Domain;
using Crud.Domain.Repositories;
using Crud.Domain.Services;
using Crud.Domain.ValueObjects;
using FluentValidation;
using VSlices.CrossCutting.AspNetCore.DataAnnotationMiddleware;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Update;

public sealed class UpdateQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddEndpoint<EndpointDefinition>()
            .AddLoggingBehaviorFor<Command>()
                .UsingSpanishTemplate()
            .AddFluentValidationBehaviorUsing<Validator>()
            .AddExceptionHandlingBehavior<LoggingExceptionHandlerPipeline<Command, Unit>>()
            .AddHandler<Handler>();
    }
}

public sealed record UpdateQuestionContract(
    [property: Required(ErrorMessage = "La pregunta es obligatoria")]
    string Text);

internal sealed record Command(QuestionId Id, NonEmptyString Text) : IRequest<Unit>;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions/{id:Guid}";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapPut(Path, Handler)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .DataAnnotationsValidate<UpdateQuestionContract>()
            .WithName("UpdateQuestion");
    }

    public IResult Handler(
        [FromBody]
        UpdateQuestionContract contract,
        [FromRoute]
        Guid id,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Command command = new(new QuestionId(id), new NonEmptyString(contract.Text));

        return runner
            .Run(command, cancellationToken)
            .MatchResult(TypedResults.Ok);
    }
}

internal sealed class Handler : IHandler<Command>
{
    public Eff<HandlerRuntime, Unit> Define(Command request) =>
        from token in cancelToken
        from repository in provide<IQuestionRepository>()
        from manager in provide<QuestionManager>()
        from exists in repository.Exists(request.Id)
        from _ in exists
                      ? from question in repository.Read(request.Id)
                        from _1 in liftEff(() => question.UpdateState(request.Text))
                        from _2 in manager.Update(question)
                        select unit
                      : manager.Create(request.Id, request.Text)
        select unit;
}

internal sealed class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Text)
            .NotEmpty();
    }
}
