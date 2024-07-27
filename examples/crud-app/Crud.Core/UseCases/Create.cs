using Crud.CrossCutting.Pipelines;
using Crud.Domain;
using Crud.Domain.Services;
using Crud.Domain.ValueObjects;
using FluentValidation;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Create;

public sealed class CreateQuestionDependencies : IFeatureDependencies
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

public sealed record CreateQuestionContract(string Text);

internal sealed record Command(NonEmptyString Text) : IRequest<Unit>;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapPost(Path, Handler)
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .WithName("CreateQuestion");
    }

    public IResult Handler(
        [FromBody]
        CreateQuestionContract contract,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Command command = new(contract.Text.ToNonEmpty());

        return runner
            .Run(command, cancellationToken)
            .MatchResult(_ => TypedResults.Created());
    }
}

internal sealed class Handler : IHandler<Command, Unit>
{
    public Eff<HandlerRuntime, Unit> Define(Command request) =>
        from manager in provide<QuestionManager>()
        from _ in manager.Create(request.Text)
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