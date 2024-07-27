using System.Runtime.Serialization;
using LanguageExt.Traits;

namespace Crud.Domain.ValueObjects;

public sealed class NonEmptyString : NewType<NonEmptyString, string, NonEmptyString>, 
                                     Pred<string>
{
    public NonEmptyString(string value) : base(value) { }

    public NonEmptyString(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public static bool True(string value) => string.IsNullOrWhiteSpace(value) == false;
}