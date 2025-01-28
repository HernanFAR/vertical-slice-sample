using System.Linq.Expressions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using VSlices.Base;
using VSlices.Domain.Interfaces;
using VSlices.Domain.Repository;
using static VSlices.VSlicesPrelude;
using static LanguageExt.Prelude;

namespace VSlices.Infrastructure.Domain.EntityFrameworkCore;

/// <summary>
/// EF Core implementation of the <see cref="IRepository{TKey, TRoot}"/>
/// </summary>
public abstract class EfCoreRepository<TDbContext, TRoot, TKey, TProjection> : IRepository<TRoot, TKey>
    where TDbContext : DbContext
    where TRoot : class, IAggregateRoot<TKey>
    where TKey : class, IEquatable<TKey>
    where TProjection : class
{
    /// <summary>
    /// Provide a way to filter the projection using the <typeparamref name="TRoot"/> id
    /// </summary>
    protected abstract Expression<Func<TProjection, bool>> DomainKeySelector(TKey id);

    /// <summary>
    /// Provide a way to map the projection to the <typeparamref name="TRoot"/>
    /// </summary>
    protected abstract TRoot ToDomain(TProjection projection);

    /// <summary>
    /// Provide a way to map the <typeparamref name="TRoot"/> to the projection
    /// </summary>
    protected abstract TProjection ToProjection(TRoot root);

    /// <inheritdoc />
    public Eff<VSlicesRuntime, TRoot> Get(TKey id) =>
        from context in provide<TDbContext>()
        from token in cancelToken
        from root in liftEff(async () =>
        {
            var entity = await context.Set<TProjection>()
                                      .Where(DomainKeySelector(id))
                                      .SingleAsync(token);

            return ToDomain(entity);
        })
        select root;

    /// <inheritdoc />
    public Eff<VSlicesRuntime, Option<TRoot>> GetOrOption(TKey id) =>
        from context in provide<TDbContext>()
        from token in cancelToken
        from root in liftEff(async () =>
        {
            TProjection? entity = await context.Set<TProjection>()
                                               .Where(DomainKeySelector(id))
                                               .SingleOrDefaultAsync(token);

            return entity is null 
                ? Option<TRoot>.None 
                : Option<TRoot>.Some(ToDomain(entity));
        })
        select root;

    /// <inheritdoc />
    public Eff<VSlicesRuntime, TRoot> Add(TRoot root) =>
        from context in provide<TDbContext>()
        from token in cancelToken
        from addedRoot in liftEff(async () =>
        {
            TProjection projection = ToProjection(root);

            await context.Set<TProjection>()
                         .AddAsync(projection);

            return ToDomain(projection);
        })
        select addedRoot;

    /// <inheritdoc />
    public Eff<VSlicesRuntime, TRoot> Update(TRoot root) =>
        from context in provide<TDbContext>()
        from token in cancelToken
        from updatedRoot in liftEff(() =>
        {
            TProjection projection = ToProjection(root);

            context.Set<TProjection>()
                   .Update(projection);

            return ToDomain(projection);
        })
        select updatedRoot;

    /// <inheritdoc />
    public Eff<VSlicesRuntime, TRoot> Delete(TRoot root) =>
        from context in provide<TDbContext>()
        from token in cancelToken
        from removedRoot in liftEff(() =>
        {
            TProjection projection = ToProjection(root);

            context.Set<TProjection>()
                   .Remove(projection);

            return ToDomain(projection);
        })
        select removedRoot;

    /// <inheritdoc />
    public Eff<VSlicesRuntime, bool> Exists(TKey id) =>
        from context in provide<TDbContext>()
        from token in cancelToken
        from exists in liftEff(() => context.Set<TProjection>()
                                            .Where(DomainKeySelector(id))
                                            .AnyAsync(token))
        select exists;
}
