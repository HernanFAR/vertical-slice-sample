﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VSlices.Core.Abstracts.DataAccess;
using VSlices.Core.Abstracts.Presentation;
using VSlices.Core.Abstracts.Responses;
using VSlices.Core.BusinessLogic.FluentValidation;
using VSlices.Core.DataAccess;
using VSlices.Core.DataAccess.EntityFramework;

namespace Sample.Core.UseCases;

// Presentación
public class RemoveQuestionEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/api/question/{id}", RemoveQuestionAsync)
            .WithSwaggerOperationInfo("Elimina pregunta", "Elimina una pregunta, en base a su identificador")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

    }

    public static void DefineDependencies(IServiceCollection services)
    {
        services.AddScoped<RemoveQuestionHandler>();
        services.AddScoped<IRemoveQuestionRepository, RemoveQuestionRepository>();
    }

    public static async ValueTask<IResult> RemoveQuestionAsync(
        [FromRoute] string id,
        [FromServices] IHttpContextAccessor contextAccessor,
        [FromServices] RemoveQuestionHandler handler,
        CancellationToken cancellationToken)
    {
        _ = Guid.TryParse(id, out var questionId);

        var command = new RemoveQuestionCommand(
            questionId,
            contextAccessor.GetIdentityId());

        var response = await handler.HandleAsync(command, cancellationToken);

        return response.MatchEndpointResult(_ => Results.NoContent());
    }
}

// Lógica
public record RemoveQuestionCommand(Guid Id, Guid RemovedById);

public class RemoveQuestionHandler : FullyFluentValidatedRemoveHandler<RemoveQuestionCommand, Question>
{
    private readonly IRemoveQuestionRepository _repository;

    public RemoveQuestionHandler(
        IValidator<RemoveQuestionCommand> contractValidator,
        IValidator<Question> entityValidator,
        IRemoveQuestionRepository repository) : base(contractValidator, entityValidator, repository)
    {
        _repository = repository;
    }

    protected override async ValueTask<OneOf<Success, BusinessFailure>> ValidateUseCaseRulesAsync(RemoveQuestionCommand request, CancellationToken cancellationToken)
    {
        var exist = await _repository.AnyAsync(request.Id, cancellationToken);

        if (!exist)
        {
            return BusinessFailure.Of.NotFoundResource();
        }

        return new Success();
    }

    protected override async ValueTask<Question> GetAndProcessEntityAsync(RemoveQuestionCommand request, CancellationToken cancellationToken)
        => await _repository.GetAsync(request.Id, cancellationToken);

}

public class RemoveQuestionValidator : AbstractValidator<RemoveQuestionCommand>
{
    public RemoveQuestionValidator()
    {
        RuleFor(e => e.RemovedById)
            .NotEmpty();

    }
}

public interface IRemoveQuestionRepository : IRemoveRepository<Question>
{
    ValueTask<bool> AnyAsync(Guid id, CancellationToken cancellationToken = default);

    ValueTask<Question> GetAsync(Guid id, CancellationToken cancellationToken = default);

}

public class RemoveQuestionRepository : EFRemoveRepository<ApplicationDbContext, Question>, IRemoveQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public RemoveQuestionRepository(ApplicationDbContext context, ILogger<RemoveQuestionRepository> logger)
        : base(context, logger)
    {
        _context = context;
    }

    public async ValueTask<bool> AnyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Questions
            .AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async ValueTask<Question> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Questions
            .FirstAsync(e => e.Id == id, cancellationToken);
    }
}