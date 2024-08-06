using Crud.CrossCutting;
using Microsoft.EntityFrameworkCore;
using VSlices.Base.Builder;
using VSlices.Base.Core;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Questions.Read;

public sealed record QuestionDto(Guid Id, Guid CategoryId, string Category, string Text);

public sealed record ReadQuestionsDto(QuestionDto[] Questions);

public sealed record Query : IRequest<ReadQuestionsDto>
{
    public static Query Instance { get; } = new();
}

public sealed class ReadQuestionDependencies : IFeatureDependencies<Query, ReadQuestionsDto>
{
    public static void DefineDependencies(IFeatureStartBuilder<Query, ReadQuestionsDto> feature) =>
        feature.FromIntegration.With<EndpointDefinition>()
               .Executing<Handler>()
               .AddBehaviors(chain => chain
                                      .AddLogging().UsingSpanish()
                                      .AddLoggingException().UsingSpanish());
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

internal sealed class Handler : IHandler<Query, ReadQuestionsDto>
{
    public Eff<VSlicesRuntime, ReadQuestionsDto> Define(Query input) =>
        from context in provide<AppDbContext>()
        from cancelToken in cancelToken
        from questions in liftEff(() => context
                                        .Questions
                                        .Select(x => new QuestionDto(x.Id, x.CategoryId, x.Category.Text, x.Text))
                                        .ToArrayAsync(cancelToken))
        select new ReadQuestionsDto(questions);
}
