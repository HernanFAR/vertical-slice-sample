using System.Runtime.Serialization;
using LanguageExt.Traits;
using LanguageExt.Traits.Domain;

namespace Crud.Domain.ValueObjects;

public sealed class QuestionId(Guid value) : Identifier<QuestionId>, DomainType<QuestionId, Guid>
{
    public Guid Value { get; } = value;

    public Guid To() => Value;

    public bool Equals(QuestionId? other) => other?.Value == Value;

    public override bool Equals(object? obj) => Equals(obj as QuestionId);

    public override int GetHashCode() => Value.GetHashCode();

    public static QuestionId Random() => new(Guid.NewGuid());

    public static QuestionId New(Guid repr) => new(repr);

    public static Fin<QuestionId> From(Guid repr) => Fin<QuestionId>.Succ(new QuestionId(repr));

    public static bool operator ==(QuestionId? left, QuestionId? right) => Equals(left, right);

    public static bool operator !=(QuestionId? left, QuestionId? right) => !Equals(left, right);

}
