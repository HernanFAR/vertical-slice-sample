using LanguageExt.Traits.Domain;

namespace Crud.Domain.ValueObjects;

public sealed class CategoryId(Guid value) : Identifier<CategoryId>, DomainType<CategoryId, Guid>
{
    public Guid Value { get; } = value;

    public Guid To() => Value;

    public bool Equals(CategoryId? other) => other?.Value == Value;

    public override bool Equals(object? obj) => Equals(obj as CategoryId);

    public override int GetHashCode() => Value.GetHashCode();
    
    public static CategoryId New(Guid repr) => new(repr);

    public static Fin<CategoryId> From(Guid repr) => Fin<CategoryId>.Succ(new CategoryId(repr));

    public static bool operator ==(CategoryId? left, CategoryId? right) => Equals(left, right);

    public static bool operator !=(CategoryId? left, CategoryId? right) => !Equals(left, right);
}
