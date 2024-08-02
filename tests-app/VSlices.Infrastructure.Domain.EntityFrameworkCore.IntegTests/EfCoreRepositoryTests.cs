using System.Linq.Expressions;
using System.Runtime.Serialization;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using VSlices.Domain;

namespace VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests;

public class EfCoreRepositoryTests
{
    public sealed class EntityId : NewType<EntityId, int>
    {
        public EntityId(int value) : base(value) { }

        public EntityId(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public sealed class Entity(EntityId id) : AggregateRoot<EntityId>(id)
    {

    }

    public sealed class TEntity
    {
        public int Id { get; set; }
    }

    public sealed class Context : DbContext
    {
        public DbSet<TEntity> Entities => Set<TEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("TestDb");
        }
    }

    public sealed class EntityRepository : EfCoreRepository<Context, Entity, EntityId, TEntity>
    {
        protected override Expression<Func<TEntity, bool>> DomainKeySelector(EntityId id) => 
            entity => entity.Id == id.Value;

        protected override Entity ToDomain(TEntity projection) => new(EntityId.New(projection.Id));

        protected override TEntity ToProjection(Entity root) =>
            new()
            {
                Id = root.Id.Value
            };

    }

    [Fact]
    public void Get_Success_ShouldReturnExistingProperty()
    {

    }
}
 