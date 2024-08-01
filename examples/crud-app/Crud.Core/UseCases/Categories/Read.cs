using Crud.CrossCutting;
using Crud.CrossCutting.Pipelines;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Categories.Read;

public sealed class ReadCategoriesDependencies : IFeatureDependencies
{
    public static void DefineDependencies(FeatureBuilder featureBuilder)
    {
        featureBuilder
            .AddEndpoint<EndpointDefinition>()
            .AddLoggingBehaviorFor<Query>()
                .UsingSpanishTemplate()
            .AddExceptionHandlingBehavior<LoggingExceptionHandlerPipeline<Query, ReadCategoriesDto>>()
            .AddHandler<Handler>();
    }
}

public sealed record CategoryDto(Guid Id, string Text);

public sealed record ReadCategoriesDto(CategoryDto[] Categories);

internal sealed record Query : IRequest<ReadCategoriesDto>
{
    public static Query Instance { get; } = new();
}

internal sealed class EndpointDefinition : IEndpointDefinition
{
    public const string Path = "api/categories";

    public void Define(IEndpointRouteBuilder builder)
    {
        builder.MapGet(Path, Handler)
            .Produces(StatusCodes.Status200OK)
            .WithName("ReadCategories")
            .WithTags("Categories");
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

internal sealed class Handler : IHandler<Query, ReadCategoriesDto>
{
    public Eff<VSlicesRuntime, ReadCategoriesDto> Define(Query request) =>
        from context in provide<AppDbContext>()
        from cancelToken in cancelToken
        from questions in liftEff(() => context
                                        .Categories
                                        .Select(x => new CategoryDto(x.Id, x.Text))
                                        .ToArrayAsync(cancelToken))
        select new ReadCategoriesDto(questions);
}
