﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sample.Core.Extensions;
using Sample.Core.Interfaces;
using Sample.Domain;
using Sample.Infrastructure.EntityFramework;

namespace Sample.Core.UseCases;

// Presentación
public record UpdateQuestionContract(string Name);

public class UpdateQuestionEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPatch("/api/question/{id}", UpdateQuestionAsync)
            .WithSwaggerOperationInfo("Actualiza una pregunta", "Actualiza una pregunta, en base al identificador y al body")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status422UnprocessableEntity);
    }

    public static void DefineDependencies(IServiceCollection services)
    {
        services.AddScoped<UpdateQuestionHandler>();
        services.AddScoped<IUpdateQuestionRepository, UpdateQuestionRepository>();
    }

    public static async Task<IResult> UpdateQuestionAsync(
        [FromRoute] string id,
        [FromBody] UpdateQuestionContract createQuestionContract,
        [FromServices] IHttpContextAccessor contextAccessor,
        [FromServices] UpdateQuestionHandler handler,
        CancellationToken cancellationToken)
    {
        _ = Guid.TryParse(id, out var questionId);

        var command = new UpdateQuestionCommand(
            questionId,
            createQuestionContract.Name,
            contextAccessor.GetIdentityId());

        var response = await handler.HandleAsync(command, cancellationToken);

        return response
            .Match(
                e => Results.NoContent(),
                e => Results.NotFound(),
                e => Results.UnprocessableEntity(e.Value));
    }
}

// Lógica
public record UpdateQuestionCommand(Guid Id, string Name, Guid UpdatedById);

public class UpdateQuestionHandler
{
    private readonly IUpdateQuestionRepository _repository;
    private readonly IValidator<UpdateQuestionCommand> _contractValidator;
    private readonly IValidator<Question> _domainValidator;

    public UpdateQuestionHandler(IUpdateQuestionRepository repository,
        IValidator<UpdateQuestionCommand> contractValidator,
        IValidator<Question> domainValidator)
    {
        _repository = repository;
        _contractValidator = contractValidator;
        _domainValidator = domainValidator;
    }

    public async Task<OneOf<Success, NotFound, Error<string[]>>> HandleAsync(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var contractValidationResult = await _contractValidator.ValidateAsync(request, cancellationToken);

        if (!contractValidationResult.IsValid)
        {
            return new Error<string[]>(contractValidationResult
                .Errors.Select(e => e.ErrorMessage)
                .ToArray());
        }

        var question = await _repository.GetQuestion(request.Id, cancellationToken);

        if (question is null)
        {
            return new NotFound();
        }

        question.UpdateState(request.Name, request.UpdatedById);

        var domainValidationResult = await _domainValidator.ValidateAsync(question, cancellationToken);

        if (!domainValidationResult.IsValid)
        {
            return new Error<string[]>(domainValidationResult
                .Errors.Select(e => e.ErrorMessage)
                .ToArray());
        }

        await _repository.UpdateQuestion(question, cancellationToken);

        return new Success();
    }
}

public class UpdateQuestionValidator : AbstractValidator<UpdateQuestionCommand>
{
    public UpdateQuestionValidator()
    {
        RuleFor(e => e.Name)
            .NotEmpty();

        RuleFor(e => e.UpdatedById)
            .NotEmpty();

    }
}

public interface IUpdateQuestionRepository
{
    Task<Question?> GetQuestion(Guid id, CancellationToken cancellationToken = default);

    Task UpdateQuestion(Question question, CancellationToken cancellationToken = default);

}

public class UpdateQuestionRepository : IUpdateQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public UpdateQuestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Question?> GetQuestion(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Questions
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task UpdateQuestion(Question question, CancellationToken cancellationToken = default)
    {
        _context.Update(question);

        await _context.SaveChangesAsync(cancellationToken);
    }
}