using LanguageExt.Traits.Domain;

namespace Crud.Domain.ValueObjects;

public sealed class NonEmptyString(string value) : DomainType<NonEmptyString, string>
{
    public string Value { get; } = value;

    public string To() => Value;

    public bool Equals(NonEmptyString? other) => other?.Value == Value;

    public override bool Equals(object? obj) => Equals(obj as NonEmptyString);

    public override int GetHashCode() => Value.GetHashCode();

    public static NonEmptyString New(string repr) => new(repr);

    public static Fin<NonEmptyString> From(string repr) => Fin<NonEmptyString>.Succ(new NonEmptyString(repr));

    public static bool operator ==(NonEmptyString? left, NonEmptyString? right) => Equals(left, right);

    public static bool operator !=(NonEmptyString? left, NonEmptyString? right) => !Equals(left, right);
}