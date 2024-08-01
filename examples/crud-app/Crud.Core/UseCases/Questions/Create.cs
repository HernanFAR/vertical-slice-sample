using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Extensions;
using Crud.CrossCutting.Pipelines;
using Crud.Domain;
using Crud.Domain.Rules.DataAccess;
using Crud.Domain.Rules.Services;
using Crud.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.CrossCutting.AspNetCore.DataAnnotationMiddleware;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Questions.Create;

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

public sealed record CreateQuestionContract(
    [property: DenyDefaultValue(ErrorMessage = "La categoría es obligatoria")]
    Guid CategoryId,
    [property: Required(ErrorMessage = "La pregunta es obligatorio")]
    string Text);

internal sealed record Command(CategoryId CategoryId, NonEmptyString Text) : IRequest<Unit>;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapPost(Path, Handler)
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .DataAnnotationsValidate<CreateQuestionContract>()
            .WithName("CreateQuestion")
            .WithTags("Questions");
    }

    public IResult Handler(
        [FromBody]
        CreateQuestionContract contract,
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        Command command = new(CategoryId.New(contract.CategoryId),
                              contract.Text.ToNonEmpty());

        return runner
               .Run(command, cancellationToken)
               .MatchResult(TypedResults.Created());
    }
}

internal sealed class Handler : IHandler<Command, Unit>
{
    public Eff<VSlicesRuntime, Unit> Define(Command request) =>
        from manager in provide<QuestionManager>()
        from _ in manager.Create(request.CategoryId, request.Text)
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

    private Task<bool> NotExistInDatabase(NonEmptyString name,
                                          CancellationToken cancellationToken)
    {
        Fin<bool> result = _repository.Exists(name)
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