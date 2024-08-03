using System.Linq.Expressions;
using VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.Models;

namespace VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.DataAccess;

public sealed class EntityRepository : EfCoreRepository<Context, Entity, EntityId, TEntity>
{
    protected override Expression<Func<TEntity, bool>> DomainKeySelector(EntityId id) =>
        entity => entity.Id == id.Value;

    protected override Entity ToDomain(TEntity projection) => new(EntityId.New(projection.Id), projection.Name);

    protected override TEntity ToProjection(Entity root) =>
        new()
        {
            Id = root.Id.Value,
            Name = root.Name
        };

}