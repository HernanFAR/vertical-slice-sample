namespace VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.DataAccess;

public sealed class TestUnitOfWork(EntityRepository entities) : EfCoreUnitOfWork<Context>
{
    public EntityRepository Entities { get; } = entities;
}
