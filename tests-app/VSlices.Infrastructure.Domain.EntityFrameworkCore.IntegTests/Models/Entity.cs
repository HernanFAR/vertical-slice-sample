using VSlices.Domain;

namespace VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.Models;

public sealed class Entity(EntityId id, string? name) : AggregateRoot<EntityId>(id)
{
    public string? Name { get; } = name;
}