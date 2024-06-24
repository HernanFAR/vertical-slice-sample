using System.Runtime.CompilerServices;
using Crud.CrossCutting;
using Crud.CrossCutting.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Stream;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Stream;

public sealed class StreamQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddEndpoint<EndpointDefinition>()
            .AddExceptionHandlingStreamPipeline<LoggingExceptionHandlerStreamPipeline<Query, QuestionDto>>()
            .AddStreamHandler<Handler>();
    }
}

public sealed record QuestionDto(Guid Id, string Text);

internal sealed record Query : IStream<QuestionDto>
{
    public static Query Instance { get; } = new();
}

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions/stream"; 

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapGet(Path, HandlerAsync)
            .Produces(StatusCodes.Status200OK)
            .WithName("StreamQuestion");
    }

    public async Task<IResult> HandlerAsync(
        IStreamRunner runner,
        CancellationToken cancellationToken)
    {
        return await runner
            .RunAsync(Query.Instance, cancellationToken)
            .MatchResult(TypedResults.Ok);
    }
}

internal sealed class Handler(AppDbContext context) : IStreamHandler<Query, QuestionDto>
{
    readonly AppDbContext _context = context;

    public Aff<IAsyncEnumerable<QuestionDto>> Define(Query request, CancellationToken cancellationToken = default) =>
        from questions in Eff(() => Yield(request, cancellationToken))
        select questions;

    public async IAsyncEnumerable<QuestionDto> Yield(
        Query request, 
        [EnumeratorCancellation] 
        CancellationToken cancellationToken)
    {
        QuestionDto[] allQuestions = await _context.Questions
            .Select(x => new QuestionDto(x.Id, x.Text))
            .ToArrayAsync(cancellationToken: cancellationToken);

        foreach (QuestionDto question in allQuestions)
        {
            await Task.Delay(500, cancellationToken);
            yield return question;
        }
    }
}
