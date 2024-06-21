using Crud.CrossCutting;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Read;

public sealed class ReadQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder.AddEndpoint<Endpoint>()
            .AddHandler<Handler>();
    }
}

public sealed record QuestionDto(Guid Id, string Text);

public sealed record ReadQuestionDto(QuestionDto[] Questions);

internal sealed record Query : IRequest<ReadQuestionDto>
{
    public static Query Instance { get; } = new();
}

internal sealed class Endpoint : IEndpoint
{
    public const string Path = "api/questions";

    public void DefineEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet(Path, HandlerAsync)
            .Produces(StatusCodes.Status200OK)
            .WithName("ReadQuestion");
    }

    public async Task<IResult> HandlerAsync(
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        return await runner
            .RunAsync(Query.Instance, cancellationToken)
            .MatchResult(TypedResults.Ok);
    }
}

internal sealed class Handler(AppDbContext context) : IHandler<Query, ReadQuestionDto>
{
    readonly AppDbContext _context = context;

    public Aff<ReadQuestionDto> Define(Query request, CancellationToken cancellationToken = default) =>
        from questions in Aff(async () => await _context.Questions
            .Select(x => new QuestionDto(x.Id, x.Text))
            .ToArrayAsync(cancellationToken))
        select new ReadQuestionDto(questions);

}
