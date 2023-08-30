﻿using CrossCutting.EntityFramework;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSlices.Core.Abstracts.BusinessLogic;
using VSlices.Core.Abstracts.DataAccess;
using VSlices.Core.Abstracts.Responses;
using VSlices.Core.Abstracts.Sender;
using VSlices.Core.BusinessLogic.FluentValidation;
using VSlices.Core.DataAccess.EntityFramework;
using VSlices.Core.Presentation.AspNetCore;

// ReSharper disable once CheckNamespace
namespace Core.UseCases.RemoveQuestion;

public class RemoveQuestionSwaggerDocs : SwaggerDocumentation
{
    public override string Name => "RemoveQuestion";
    public override string[] Tags => new[] { "Questions" };
    public override string Summary => "Elimina una pregunta";

    public override SwaggerResponse[] Responses => new[]
    {
        SwaggerResponse.WithStatusCode(StatusCodes.Status200OK, "Se elimino correctamente la pregunta"),
        SwaggerResponse.WithStatusCode(StatusCodes.Status404NotFound, "No se encontro la pregunta")
    };
}

public class RemoveQuestionEndpoint : AspNetCoreEndpointDefinition, IEndpointDefinition
{
    public override Delegate DelegateHandler => HandleAsync;
    public override string Route => "api/question/{id}";
    public override HttpMethod HttpMethod => HttpMethod.Delete;
    public override SwaggerDocumentation? SwaggerConfiguration => new RemoveQuestionSwaggerDocs();

    public static void DefineDependencies(IServiceCollection services)
    {
        services.AddScoped<IRemoveQuestionRepository, RemoveQuestionRepository>();
    }

    public static async Task<IResult> HandleAsync([FromServices] ISender sender,
        [FromQuery] string id,
        CancellationToken cancellationToken)
    {
        var command = new RemoveQuestionCommand(id);
        var result = await sender.SendAsync(command, cancellationToken);

        return result.MatchEndpointResult(TypedResults.Ok);
    }
}

public record RemoveQuestionCommand(string Id) : ICommand;

public class RemoveQuestionHandler : EntityFluentValidatedRemoveHandler<RemoveQuestionCommand, Question>
{
    private readonly IRemoveQuestionRepository _repository;

    public RemoveQuestionHandler(IValidator<Question> entityValidator, IRemoveQuestionRepository repository)
        : base(entityValidator, repository)
    {
        _repository = repository;
    }

    protected override async ValueTask<Response<Success>> ValidateUseCaseRulesAsync(
        RemoveQuestionCommand request, CancellationToken cancellationToken)
    {
        var exist = await _repository.ExistQuestionAsync(request.Id, cancellationToken);

        return exist
            ? Success.Value
            : BusinessFailure.Of.NotFoundResource();
    }

    protected override async ValueTask<Question> GetAndProcessEntityAsync(RemoveQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _repository.GetQuestionAsync(request.Id, cancellationToken);

        return question;
    }
}

public interface IRemoveQuestionRepository : IRemoveRepository<Question>
{
    ValueTask<bool> ExistQuestionAsync(string id, CancellationToken cancellationToken);

    ValueTask<Question> GetQuestionAsync(string id, CancellationToken cancellationToken);
}

public class RemoveQuestionRepository : EFRemoveRepository<AppDbContext, Question>, IRemoveQuestionRepository
{
    private readonly AppDbContext _context;

    public RemoveQuestionRepository(AppDbContext context, ILogger<RemoveQuestionRepository> logger) : base(context, logger)
    {
        _context = context;
    }

    public async ValueTask<bool> ExistQuestionAsync(string id, CancellationToken cancellationToken)
    {
        return await _context.Questions
            .Where(e => e.Id == id)
            .AnyAsync(cancellationToken);
    }

    public async ValueTask<Question> GetQuestionAsync(string id, CancellationToken cancellationToken)
    {
        return await _context.Questions
            .Where(e => e.Id == id)
            .SingleAsync(cancellationToken);
    }
}