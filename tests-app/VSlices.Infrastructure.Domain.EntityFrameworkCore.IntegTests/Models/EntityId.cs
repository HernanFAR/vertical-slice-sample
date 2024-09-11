using LanguageExt;
using LanguageExt.Traits.Domain;

namespace VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.Models;

public sealed class EntityId(Guid value) : Identifier<EntityId>, DomainType<EntityId, Guid>
{
    public Guid Value { get; } = value;

    public Guid To() => Value;

    public bool Equals(EntityId? other) => other?.Value == Value;

    public override bool Equals(object? obj) => Equals(obj as EntityId);

    public override int GetHashCode() => Value.GetHashCode();

    public static EntityId Random() => new(Guid.NewGuid());

    public static EntityId New(Guid repr) => new(repr);

    public static Fin<EntityId> From(Guid repr) => Fin<EntityId>.Succ(new EntityId(repr));

    public static bool operator ==(EntityId? left, EntityId? right) => Equals(left, right);

    public static bool operator !=(EntityId? left, EntityId? right) => !Equals(left, right);

}