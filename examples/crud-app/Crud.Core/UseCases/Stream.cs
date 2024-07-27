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
        builder.MapGet(Path, Handler)
            .Produces(StatusCodes.Status200OK)
            .WithName("StreamQuestion");
    }

    public IResult Handler(
        IStreamRunner runner,
        CancellationToken cancellationToken)
    {
        return runner
            .Run(Query.Instance, cancellationToken)
            .MatchResult(TypedResults.Ok);
    }
}

internal sealed class Handler(AppDbContext context) : IStreamHandler<Query, QuestionDto>
{
    readonly AppDbContext _context = context;

    public Eff<HandlerRuntime, IAsyncEnumerable<QuestionDto>> Define(Query request) =>
        from token in cancelToken
        from questions in liftEff(() => Yield(token))
        select questions;

    public async IAsyncEnumerable<QuestionDto> Yield(
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
