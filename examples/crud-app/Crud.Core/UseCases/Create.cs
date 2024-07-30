using System.ComponentModel.DataAnnotations;
using Crud.CrossCutting.Pipelines;
using Crud.Domain;
using Crud.Domain.Repositories;
using Crud.Domain.Services;
using Crud.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.CrossCutting.AspNetCore.DataAnnotationMiddleware;

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

public sealed record CreateQuestionContract(
    [property: Required(ErrorMessage = "La pregunta es obligatorio")]
    string Text);

internal sealed record Command(NonEmptyString Text) : IRequest<Unit>;

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapPost(Path, Handler)
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .DataAnnotationsValidate<CreateQuestionContract>()
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
            .MatchResult(TypedResults.Created());
    }
}

internal sealed class Handler : IHandler<Command, Unit>
{
    public Eff<VSlicesRuntime, Unit> Define(Command request) =>
        from manager in provide<QuestionManager>()
        from _ in manager.Create(request.Text)
        select unit;
}

internal sealed class Validator : AbstractValidator<Command>
{
    private readonly VSlicesRuntime _VSlicesRuntime;
    private readonly IQuestionRepository _repository;
    private readonly ILogger<Validator> _logger;

    public Validator(VSlicesRuntime VSlicesRuntime, IQuestionRepository repository, ILogger<Validator> logger)
    {
        _VSlicesRuntime = VSlicesRuntime;
        _repository     = repository;
        _logger         = logger;

        RuleFor(x => x.Text)
            .MustAsync(NotExistInDatabase).WithMessage("La pregunta ya existe en el sistema");
    }

    private Task<bool> NotExistInDatabase(NonEmptyString name,
                                          CancellationToken cancellationToken)
    {
        Fin<bool> result = _repository.Exists(name)
                                      .Run(_VSlicesRuntime, cancellationToken);

        return result.Match(exist => exist is false,
                            error =>
                            {
                                _logger.LogError("No se ha podido validar: {Error}.", error);

                                return false;
                            })
                     .AsTask();

    }
}