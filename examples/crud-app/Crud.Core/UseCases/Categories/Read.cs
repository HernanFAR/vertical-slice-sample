﻿using Crud.CrossCutting;
using Crud.CrossCutting.Pipelines;
using Microsoft.EntityFrameworkCore;
using VSlices.Base.Builder;
using VSlices.Base.Core;

// ReSharper disable once CheckNamespace
namespace Crud.Core.UseCases.Categories.Read;

public sealed record Query : IRequest<ReadCategoriesDto>
{
    public static Query Instance { get; } = new();
}

public sealed class ReadCategoriesDependencies : IFeatureDependencies<Query, ReadCategoriesDto>
{
    public static void DefineDependencies(IFeatureStartBuilder<Query, ReadCategoriesDto> define) =>
        define.FromIntegration.Using<EndpointIntegrator>()
              .Execute<RequestHandler>()
              .WithBehaviorChain(chain => chain
                                     .AddLogging().InSpanish()
                                     .AddLoggingException().InSpanish());
}

public sealed record CategoryDto(Guid Id, string Text);

public sealed record ReadCategoriesDto(CategoryDto[] Categories);

internal sealed class EndpointIntegrator : IEndpointIntegrator
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

internal sealed class RequestHandler : IHandler<Query, ReadCategoriesDto>
{
    public Eff<VSlicesRuntime, ReadCategoriesDto> Define(Query input) =>
        from context in provide<AppDbContext>()
        from cancelToken in cancelToken
        from questions in liftEff(() => context
                                        .Categories
                                        .Select(x => new CategoryDto(x.Id, x.Text))
                                        .ToArrayAsync(cancelToken))
        select new ReadCategoriesDto(questions);
}
