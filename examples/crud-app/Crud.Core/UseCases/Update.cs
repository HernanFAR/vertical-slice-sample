using Crud.Domain;
using Crud.Domain.Repositories;
using Crud.Domain.Services;
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
            .AddFluentValidationBehavior<Validator>()
            .AddHandler<Handler>();
    }
}

public sealed record UpdateQuestionContract(string Text);

internal sealed record Command(QuestionId Id, string Text) : IRequest<Unit>;

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
        Command command = new(new QuestionId(id), contract.Text);

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
    readonly IQuestionRepository _repository = repository;
    readonly QuestionManager _manager = manager;

    public Aff<Runtime, Unit> Define(Command request) =>
        from cancelToken in cancelToken<Runtime>()
        from exists in _repository.ExistsAsync(request.Id, cancelToken)
        from _ in exists
            ? _repository
                .ReadAsync(request.Id, cancelToken)
                .Bind(question => _manager.UpdateAsync(question, cancelToken))
            : _manager
                .CreateAsync(request.Text, cancelToken)
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
