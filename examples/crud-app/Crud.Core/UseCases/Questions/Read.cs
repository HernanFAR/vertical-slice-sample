using Crud.CrossCutting;
using Crud.CrossCutting.Pipelines;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Questions.Read;

public sealed class ReadQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddEndpoint<EndpointDefinition>()
            .AddLoggingBehaviorFor<Query>()
                .UsingSpanishTemplate()
            .AddExceptionHandlingBehavior<LoggingExceptionHandlerPipeline<Query, ReadQuestionsDto>>()
            .AddRequestHandler<RequestHandler>();
    }
}

public sealed record QuestionDto(Guid Id, Guid CategoryId, string Category, string Text);

public sealed record ReadQuestionsDto(QuestionDto[] Questions);

internal sealed record Query : IRequest<ReadQuestionsDto>
{
    public static Query Instance { get; } = new();
}

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/questions";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapGet(Path, Handler)
            .Produces(StatusCodes.Status200OK)
            .WithName("ReadQuestion")
            .WithTags("Questions");
    }

    public IResult Handler(
        IRequestRunner runner,
        CancellationToken cancellationToken)
    {
        return runner
            .Run(Query.Instance, cancellationToken)
            .MatchResult(TypedResults.Ok);
    }
}

internal sealed class RequestHandler : IRequestHandler<Query, ReadQuestionsDto>
{
    public Eff<VSlicesRuntime, ReadQuestionsDto> Define(Query request) =>
        from context in provide<AppDbContext>()
        from cancelToken in cancelToken
        from questions in liftEff(() => context
                                        .Questions
                                        .Select(x => new QuestionDto(x.Id, x.CategoryId, x.Category.Text, x.Text))
                                        .ToArrayAsync(cancelToken))
        select new ReadQuestionsDto(questions);
}
