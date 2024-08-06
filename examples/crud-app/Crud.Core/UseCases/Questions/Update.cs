using Crud.CrossCutting.Pipelines;
using Crud.Domain.Rules.Services;
using Crud.Domain.ValueObjects;
using Crud.Domain;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Crud.Domain.Rules.DataAccess;
using VSlices.CrossCutting.AspNetCore.DataAnnotationMiddleware;
using System.ComponentModel.DataAnnotations.Extensions;
using VSlices.Base.Builder;
using VSlices.Base.Core;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Questions.Update;

public sealed record Command(QuestionId Id, CategoryId CategoryId, NonEmptyString Text) : IRequest;

public sealed class UpdateQuestionDependencies : IFeatureDependencies<Command>
{
    public static void DefineDependencies(IFeatureStartBuilder<Command, Unit> feature) =>
        feature.FromIntegration.With<EndpointDefinition>()
               .Executing<Handler>()
               .AddBehaviors(chain => chain
                                      .AddLogging().UsingSpanish()
                                      .AddFluentValidationUsing<Validator>()
                                      .AddLoggingException().UsingSpanish());
}

public sealed record UpdateQuestionContract(
    [property: DenyDefaultValue(ErrorMessage = "La categoría es obligatoria")]
    Guid CategoryId,
    [property: Required(ErrorMessage = "La pregunta es obligatoria")]
    string Text);

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
            .WithName("UpdateQuestion")
            .WithTags("Questions");
    }

    public IResult Handler(
        [FromBody]
        UpdateQuestionContract contract,
        [FromRoute]
        Guid id,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Command command = new(QuestionId.New(id),
                              CategoryId.New(contract.CategoryId),
                              NonEmptyString.New(contract.Text));

        return runner
            .Run(command, cancellationToken)
            .MatchResult(TypedResults.Ok);
    }
}

internal sealed class Handler : IHandler<Command>
{
    public Eff<VSlicesRuntime, Unit> Define(Command input) =>
        from token in cancelToken
        from repository in provide<IQuestionRepository>()
        from manager in provide<QuestionManager>()
        from exists in repository.Exists(input.Id)
        from _ in exists
            ? from question in repository.Get(input.Id)
              from _1 in liftEff(() => question.UpdateState(input.CategoryId, input.Text))
              from _2 in manager.Update(question)
              select unit
            : manager.Create(input.Id, input.CategoryId, input.Text)
        select unit;
}

internal sealed class Validator : AbstractValidator<Command>
{
    private readonly VSlicesRuntime _runtime;
    private readonly IQuestionRepository _repository;
    private readonly ILogger<Validator> _logger;

    public Validator(VSlicesRuntime runtime, IQuestionRepository repository, ILogger<Validator> logger)
    {
        _runtime = runtime;
        _repository = repository;
        _logger = logger;

        RuleFor(x => x.Text)
            .MustAsync(NotExistInDatabase).WithMessage("La pregunta ya existe en el sistema");

        RuleFor(x => x.CategoryId)
            .Must(x => CategoryType.FindOrOption(x).IsSome).WithMessage("La categoría no existe");
    }

    private Task<bool> NotExistInDatabase(Command command,
                                          NonEmptyString name,
                                          CancellationToken cancellationToken)
    {
        Fin<bool> result = _repository.Exists(command.Id, name)
                                      .Run(_runtime, cancellationToken);

        return result.Match(exist => exist is false,
                            error =>
                            {
                                _logger.LogError("No se ha podido validar: {Error}.", error);

                                return false;
                            })
                     .AsTask();

    }
}
