using LanguageExt.TypeClasses;
using System.Runtime.Serialization;

namespace Crud.Domain.ValueObjects;

public struct NonEmptyStringPred : Pred<string>
{
    public bool True(string value) => string.IsNullOrWhiteSpace(value) == false;
}

public sealed class NonEmptyString : NewType<NonEmptyString, string, NonEmptyStringPred>
{
    public NonEmptyString(string value) : base(value) { }

    public NonEmptyString(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
