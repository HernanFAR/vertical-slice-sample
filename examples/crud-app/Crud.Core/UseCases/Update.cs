using Crud.CrossCutting.Pipelines;
using Crud.Domain;
using Crud.Domain.Repositories;
using Crud.Domain.Services;
using Crud.Domain.ValueObjects;
using FluentValidation;
using LanguageExt.SysX.Live;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Update;

public sealed class UpdateQuestionDependencies : IFeatureDependencies
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

public sealed record UpdateQuestionContract(string Text);

internal sealed record Command(QuestionId Id, NonEmptyString Text) : IRequest<Unit>;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions/{id:Guid}";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapPut(Path, HandlerAsync)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .WithName("UpdateQuestion");
    }

    public async Task<IResult> HandlerAsync(
        [FromBody]
        UpdateQuestionContract contract,
        [FromRoute]
        Guid id,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Command command = new(new QuestionId(id), new NonEmptyString(contract.Text));

        return await runner
            .RunAsync(command, cancellationToken)
            .MatchResult(TypedResults.Ok);
    }
}

internal sealed class Handler(
    IQuestionRepository repository,
    QuestionManager manager)
    : IHandler<Command>
{
    private readonly IQuestionRepository _repository = repository;
    private readonly QuestionManager _manager = manager;

    public Aff<Runtime, Unit> Define(Command request) =>
        from cancelToken in cancelToken<Runtime>()
        from exists in _repository.Exists(request.Id)
        from _ in exists
            ? from question in _repository.Read(request.Id)
            from _1 in Eff(() => question.UpdateState(request.Text))
            from _2 in _manager.Update(question)
            select unit
            : _manager.Create(request.Id, request.Text)
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
