using Crud.CrossCutting;
using Crud.CrossCutting.Pipelines;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Read;

public sealed class ReadQuestionDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddEndpoint<EndpointDefinition>()
            .AddLoggingBehaviorFor<Query>()
                .UsingSpanishTemplate()
            .AddExceptionHandlingBehavior<LoggingExceptionHandlerPipeline<Query, ReadQuestionDto>>()
            .AddHandler<Handler>();
    }
}

public sealed record QuestionDto(Guid Id, string Text);

public sealed record ReadQuestionDto(QuestionDto[] Questions);

internal sealed record Query : IRequest<ReadQuestionDto>
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
            .WithName("ReadQuestion");
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

internal sealed class Handler : IHandler<Query, ReadQuestionDto>
{
    public Eff<HandlerRuntime, ReadQuestionDto> Define(Query request) =>
        from context in provide<AppDbContext>()
        from cancelToken in cancelToken
        from questions in liftEff(() => context
                                        .Questions
                                        .Select(x => new QuestionDto(x.Id, x.Text))
                                        .ToArrayAsync(cancelToken))
        select new ReadQuestionDto(questions);
}
