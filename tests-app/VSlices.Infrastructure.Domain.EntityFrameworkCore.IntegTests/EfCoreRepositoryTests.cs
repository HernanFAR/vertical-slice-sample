using FluentAssertions;
using LanguageExt;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using VSlices.Base;
using VSlices.Base.Traits;
using VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.DataAccess;
using VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.Models;

namespace VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests;

public class EfCoreRepositoryTests(Fixture fixture) : IClassFixture<Fixture>
{
    private readonly Fixture _fixture = fixture;

    [Fact]
    public async Task Get_Success_ShouldReturnExistingEntityAsync()
    {
        // Arrange
        EntityRepository repository = new();

        Entity expEntity = new(EntityId.Random(), null);
        TEntity entity = new()
        {
            Id = expEntity.Id.Value
        };

        _fixture.Context.Add(entity);
        await _fixture.Context.SaveChangesAsync();
        _fixture.Context.ChangeTracker.Clear();

        DependencyProvider dependencyProvider = new(_fixture.Provider);

        Eff<VSlicesRuntime, Entity> eff = repository.Get(EntityId.New(entity.Id));

        // Act
        Fin<Entity> result = eff.Run(VSlicesRuntime.New(dependencyProvider),
                             EnvIO.New());

        // Assert
        _ = result.BiMap(Succ: e =>
                         {
                             e.Id.Value.Should().Be(expEntity.Id.Value);
                             e.Name.Should().BeNull();

                             return e;
                         },
                         Fail: _ => throw new UnreachableException());
    }

    [Fact]
    public void Get_Failure_ShouldThrow()
    {
        // Arrange
        EntityRepository repository = new();

        DependencyProvider dependencyProvider = new(_fixture.Provider);

        Eff<VSlicesRuntime, Entity> eff = repository.Get(EntityId.Random());

        // Act
        Fin<Entity> result = eff.Run(VSlicesRuntime.New(dependencyProvider),
                             EnvIO.New());

        // Assert
        _ = result.BiMap<int>(Succ: e => throw new UnreachableException(),
                         Fail: e =>
                         {
                             e.IsExceptional.Should()
                              .BeTrue();

                             e.ToException()
                              .Should()
                              .BeOfType<InvalidOperationException>()
                              .Subject.Message.Should()
                              .Be("Sequence contains no elements.");

                             return e;
                         });

    }

    [Fact]
    public async Task GetOrOption_Success_ShouldReturnExistingEntityAsync()
    {
        // Arrange
        EntityRepository repository = new();

        Entity expEntity = new(EntityId.Random(), null);
        TEntity entity = new()
        {
            Id = expEntity.Id.Value
        };

        _fixture.Context.Add(entity);
        await _fixture.Context.SaveChangesAsync();
        _fixture.Context.ChangeTracker.Clear();

        DependencyProvider dependencyProvider = new(_fixture.Provider);

        Eff<VSlicesRuntime, Option<Entity>> eff = repository.GetOrOption(EntityId.New(entity.Id));

        // Act
        Fin<Option<Entity>> result = eff.Run(VSlicesRuntime.New(dependencyProvider),
                                             EnvIO.New());

        // Assert
        _ = result.BiMap(Succ: e =>
                         {
                             e.IsSome.Should().BeTrue();
                             e.IsNone.Should().BeFalse();
                             e.IfSome(entity =>
                             {
                                 entity.Id.Value.Should().Be(expEntity.Id.Value);
                                 entity.Name.Should().BeNull();
                             });

                             return e;
                         },
                         Fail: _ => throw new UnreachableException());
    }

    [Fact]
    public void GetOrOption_Success_ShouldReturnNone()
    {
        // Arrange
        EntityRepository repository = new();

        DependencyProvider dependencyProvider = new(_fixture.Provider);

        var eff = repository.GetOrOption(EntityId.Random());

        // Act
        var result = eff.Run(VSlicesRuntime.New(dependencyProvider),
                             EnvIO.New());

        // Assert
        _ = result.BiMap(Succ: e =>
                         {
                             e.IsSome.Should().BeFalse();
                             e.IsNone.Should().BeTrue();

                             return e;
                         },
                         Fail: _ => throw new UnreachableException());

    }

    [Fact]
    public async Task AddAndSaveChanges_Success_ShouldThrow()
    {
        // Arrange
        EntityRepository repository = new();
        TestUnitOfWork   unitOfWork = new(repository);
        Entity           entity     = new Entity(EntityId.Random(), null);

        DependencyProvider dependencyProvider = new(_fixture.Provider);

        Eff<VSlicesRuntime, Unit> eff = from _ in unitOfWork.Entities.Add(entity)
                                        from _1 in unitOfWork.SaveChanges()
                                        select Prelude.unit;

        // Act
        Fin<Unit> result = eff.Run(VSlicesRuntime.New(dependencyProvider),
                                   EnvIO.New());

        // Assert
        result.IsSucc.Should().BeTrue();

        var context = _fixture.Context;
        var created = await context.Entities
                                   .Where(e => e.Id == entity.Id.Value)
                                   .SingleAsync();

        created.Id.Should().Be(entity.Id.Value);
        created.Name.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAndSaveChanges_Success_ShouldThrow()
    {
        // Arrange
        var              context       = _fixture.Context;

        EntityId         id            = EntityId.Random();
        EntityRepository repository    = new();
        TestUnitOfWork   unitOfWork    = new(repository);
        Entity           updatedEntity = new(id, "Ahora si existe");

        context.Entities.Add(new TEntity { Id = id.Value, Name = null });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        DependencyProvider dependencyProvider = new(_fixture.Provider);

        Eff<VSlicesRuntime, Unit> eff = from _ in unitOfWork.Entities.Update(updatedEntity)
                                        from _1 in unitOfWork.SaveChanges()
                                        select Prelude.unit;

        // Act
        Fin<Unit> result = eff.Run(VSlicesRuntime.New(dependencyProvider),
                                   EnvIO.New());

        // Assert
        result.IsSucc.Should().BeTrue();

        var updated = await context.Entities
                                   .Where(e => e.Id == updatedEntity.Id.Value)
                                   .SingleAsync();

        updated.Id.Should().Be(id.Value);
        updated.Name.Should().Be(updatedEntity.Name);
    }

    [Fact]
    public async Task DeleteAndSaveChanges_Success_ShouldThrow()
    {
        // Arrange
        var context = _fixture.Context;

        EntityId         id            = EntityId.Random();
        EntityRepository repository    = new();
        TestUnitOfWork   unitOfWork    = new(repository);
        Entity           updatedEntity = new(id, null);

        context.Entities.Add(new TEntity { Id = id.Value, Name = null });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        DependencyProvider dependencyProvider = new(_fixture.Provider);

        Eff<VSlicesRuntime, Unit> eff = from _ in unitOfWork.Entities.Delete(updatedEntity)
                                        from _1 in unitOfWork.SaveChanges()
                                        select Prelude.unit;

        // Act
        Fin<Unit> result = eff.Run(VSlicesRuntime.New(dependencyProvider),
                                   EnvIO.New());

        // Assert
        result.IsSucc.Should().BeTrue();

        var exist = await context.Entities
                                   .Where(e => e.Id == updatedEntity.Id.Value)
                                   .AnyAsync();

        exist.Should().BeFalse();
    }
}
