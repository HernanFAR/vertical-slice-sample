using Crud.CrossCutting.Pipelines;
using Crud.Domain.Services;
using Crud.Domain.ValueObjects;
using FluentValidation;
using LanguageExt.SysX.Live;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Create;

public sealed class CreateQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddEndpoint<EndpointDefinition>()
            .AddLoggingFor<Command>().UsingSpanishTemplate()
            .AddFluentValidationUsing<Validator>()
            .AddExceptionHandling<LoggingExceptionHandlerPipeline<Command, Unit>>()
            .AddHandler<Handler>();
    }
}

public sealed record CreateQuestionContract(string Text);

internal sealed record Command(string Text) : IRequest<Unit>;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapPost(Path, HandlerAsync)
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .WithName("CreateQuestion");
    }

    public async Task<IResult> HandlerAsync(
        [FromBody]
        CreateQuestionContract contract,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Command command = new(contract.Text);

        return await runner
            .RunAsync(command, cancellationToken)
            .MatchResult(_ => TypedResults.Created());
    }
}

internal sealed class Handler(QuestionManager manager) : IHandler<Command, Unit>
{
    private readonly QuestionManager _manager = manager;

    public Aff<Runtime, Unit> Define(Command request) =>
        from _ in _manager.Create(new NonEmptyString(request.Text))
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